using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using RollChockBackend.Models;

namespace RollChockBackend.Repositories
{
    public class ChockRepository : IChockRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<ChockRepository> _logger;

        public ChockRepository(IConfiguration configuration, ILogger<ChockRepository> logger)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("OracleConnection")
                ?? throw new InvalidOperationException("OracleConnection string is not configured.");
        }

        public async Task<ChockLookupsDto> GetLookupsAsync()
        {
            var result = new ChockLookupsDto();
            using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync();

            // Chock Type dropdown: CD_TYPE = 'C0032'
            using (var cmd = new OracleCommand(
                "SELECT CD_VALUE FROM T_CODES WHERE CD_TYPE = 'C0032' ORDER BY 1", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    result.ChockTypes.Add(reader["CD_VALUE"]?.ToString() ?? string.Empty);
                }
            }

            // Chock Maker dropdown: CD_TYPE = 'CHKMK'
            using (var cmd = new OracleCommand(
                "SELECT CD_VALUE, CD_DESC FROM T_CODES WHERE CD_TYPE = 'CHKMK' ORDER BY 1", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    result.ChockMakers.Add(new CodeDto
                    {
                        CodeValue = reader["CD_VALUE"]?.ToString() ?? string.Empty,
                        CodeDesc = reader["CD_DESC"]?.ToString() ?? string.Empty
                    });
                }
            }

            return result;
        }

        public async Task<ChockQueryResponse> QueryChockAsync(string chockId, string chockType)
        {
            using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync();

            var response = new ChockQueryResponse { Found = false };

            // Mirrors the legacy EXECUTE-QUERY trigger's first SELECT against
            // V_CHOCK_MAST, filtered to a non-deleted row for this chock.
            const string chockSql = @"
                SELECT * FROM T_CHOCK_MAST
                WHERE CHM_ID_CHOCK = :ChockId
                  AND CHM_CHK_TYP  = :ChockType
                  AND CHM_DEL_TAG  = 'N'";

            using (var cmd = new OracleCommand(chockSql, conn))
            {
                cmd.Parameters.Add(new OracleParameter("ChockId", OracleDbType.Varchar2) { Value = chockId });
                cmd.Parameters.Add(new OracleParameter("ChockType", OracleDbType.Varchar2) { Value = chockType });

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    response.Found = true;
                    response.Chock = ReadChockRecord(reader);
                }
            }

            if (!response.Found)
            {
                return response;
            }

            // Mirrors the second block: status description from T_CODES (C0030).
            using (var cmd = new OracleCommand(
                "SELECT CD_DESC FROM T_CODES WHERE CD_TYPE = 'C0030' AND CD_VALUE = :Status", conn))
            {
                cmd.Parameters.Add(new OracleParameter("Status", OracleDbType.Varchar2)
                {
                    Value = (object?)response.Chock!.CHM_CD_CHK_PROG ?? DBNull.Value
                });
                var desc = await cmd.ExecuteScalarAsync();
                response.StatusDesc = desc?.ToString();
            }

            // Mirrors the third block: tolerance standards from T_CHOCK_STND,
            // keyed by chock type + first 3 chars of the chock id.
            var osDs = chockId.Length >= 3 ? chockId.Substring(0, 3) : chockId;
            const string tolSql = @"
                SELECT * FROM T_CHOCK_STND
                WHERE CHS_CHK_TYP = :ChockType
                  AND CHS_OS_DS   = :OsDs";

            using (var cmd = new OracleCommand(tolSql, conn))
            {
                cmd.Parameters.Add(new OracleParameter("ChockType", OracleDbType.Varchar2) { Value = chockType });
                cmd.Parameters.Add(new OracleParameter("OsDs", OracleDbType.Varchar2) { Value = osDs });

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    response.Tolerance = new ChockToleranceDto
                    {
                        CHS_CK_IDI_TL_U = GetDecimal(reader, "CHS_CK_IDI_TL_U"),
                        CHS_CK_IDI_TL_L = GetDecimal(reader, "CHS_CK_IDI_TL_L"),
                        CHS_CK_END_TL_U = GetDecimal(reader, "CHS_CK_END_TL_U"),
                        CHS_CK_END_TL_L = GetDecimal(reader, "CHS_CK_END_TL_L"),
                        CHS_CK_W_LIN_TL_U = GetDecimal(reader, "CHS_CK_W_LIN_TL_U"),
                        CHS_CK_W_LIN_TL_L = GetDecimal(reader, "CHS_CK_W_LIN_TL_L"),
                        CHS_CK_W_LIN_TL_U1 = GetDecimal(reader, "CHS_CK_W_LIN_TL_U1"),
                        CHS_CK_W_LIN_TL_L1 = GetDecimal(reader, "CHS_CK_W_LIN_TL_L1"),
                        CHS_CK_LIN_TL_U = GetDecimal(reader, "CHS_CK_LIN_TL_U"),
                        CHS_CK_LIN_TL_L = GetDecimal(reader, "CHS_CK_LIN_TL_L"),
                        CHS_CK_LIN_TL_U1 = GetDecimal(reader, "CHS_CK_LIN_TL_U1"),
                        CHS_CK_LIN_TL_L1 = GetDecimal(reader, "CHS_CK_LIN_TL_L1")
                    };
                }
            }

            return response;
        }

        // Drives the "select Chock Type -> some boxes grey out, status
        // auto-fills" behavior. Looks up the T_CHOCK_STND row for this type
        // (narrowed to the Chock ID's 3-char prefix once one is known) and:
        //   - reports the default status (CNEW / its T_CODES description)
        //     for brand-new chocks
        //   - reports which measurement fields have no tolerance defined for
        //     this type, so the frontend can disable them instead of asking
        //     the user to fill in a value nothing will validate against
        public async Task<ChockTypeConfigDto> GetTypeConfigAsync(string chockType, string? chockId)
        {
            var config = new ChockTypeConfigDto();
            using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync();

            // Default status for a new chock: CNEW plus its T_CODES (C0030) description.
            using (var cmd = new OracleCommand(
                "SELECT CD_DESC FROM T_CODES WHERE CD_TYPE = 'C0030' AND CD_VALUE = :Status", conn))
            {
                cmd.Parameters.Add(new OracleParameter("Status", OracleDbType.Varchar2) { Value = config.DefaultStatusCode });
                var desc = await cmd.ExecuteScalarAsync();
                config.DefaultStatusDesc = desc?.ToString();
            }

            // Tolerance standard row for this type. If a Chock ID prefix is
            // known, use the exact CHS_OS_DS match first (mirrors the legacy
            // SUBSTR(CHM_ID_CHOCK,1,3) lookup); otherwise fall back to any
            // row on file for the type, since the "which fields apply" set
            // is a property of the chock type/design, not the mill prefix.
            var osDs = !string.IsNullOrWhiteSpace(chockId) && chockId!.Length >= 3
                ? chockId.Substring(0, 3)
                : null;

            ChockToleranceDto? tolerance = null;

            if (osDs != null)
            {
                tolerance = await ReadToleranceRow(conn,
                    "SELECT * FROM T_CHOCK_STND WHERE CHS_CHK_TYP = :ChockType AND CHS_OS_DS = :OsDs",
                    chockType, osDs);
            }

            if (tolerance == null)
            {
                tolerance = await ReadToleranceRow(conn,
                    "SELECT * FROM T_CHOCK_STND WHERE CHS_CHK_TYP = :ChockType ORDER BY CHS_OS_DS FETCH FIRST 1 ROWS ONLY",
                    chockType, null);
            }

            config.Tolerance = tolerance;
            config.DisabledFields = ComputeDisabledFields(tolerance);

            return config;
        }

        private static async Task<ChockToleranceDto?> ReadToleranceRow(
            OracleConnection conn, string sql, string chockType, string? osDs)
        {
            using var cmd = new OracleCommand(sql, conn);
            cmd.Parameters.Add(new OracleParameter("ChockType", OracleDbType.Varchar2) { Value = chockType });
            if (osDs != null)
            {
                cmd.Parameters.Add(new OracleParameter("OsDs", OracleDbType.Varchar2) { Value = osDs });
            }

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            return new ChockToleranceDto
            {
                CHS_CK_IDI_TL_U = GetDecimal(reader, "CHS_CK_IDI_TL_U"),
                CHS_CK_IDI_TL_L = GetDecimal(reader, "CHS_CK_IDI_TL_L"),
                CHS_CK_END_TL_U = GetDecimal(reader, "CHS_CK_END_TL_U"),
                CHS_CK_END_TL_L = GetDecimal(reader, "CHS_CK_END_TL_L"),
                CHS_CK_W_LIN_TL_U = GetDecimal(reader, "CHS_CK_W_LIN_TL_U"),
                CHS_CK_W_LIN_TL_L = GetDecimal(reader, "CHS_CK_W_LIN_TL_L"),
                CHS_CK_W_LIN_TL_U1 = GetDecimal(reader, "CHS_CK_W_LIN_TL_U1"),
                CHS_CK_W_LIN_TL_L1 = GetDecimal(reader, "CHS_CK_W_LIN_TL_L1"),
                CHS_CK_LIN_TL_U = GetDecimal(reader, "CHS_CK_LIN_TL_U"),
                CHS_CK_LIN_TL_L = GetDecimal(reader, "CHS_CK_LIN_TL_L"),
                CHS_CK_LIN_TL_U1 = GetDecimal(reader, "CHS_CK_LIN_TL_U1"),
                CHS_CK_LIN_TL_L1 = GetDecimal(reader, "CHS_CK_LIN_TL_L1"),
            };
        }

        // Maps each tolerance-pair on T_CHOCK_STND to the form fields it
        // governs. If a pair is NULL for this type's standard row, there is
        // no tolerance to check the measurement against, so those fields are
        // reported as not applicable (frontend greys them out and excludes
        // them from validation/save). If no standard row exists at all for
        // the type yet, nothing is disabled — we don't want missing seed
        // data to silently lock out every field.
        private static List<string> ComputeDisabledFields(ChockToleranceDto? t)
        {
            var disabled = new List<string>();
            if (t == null) return disabled;

            if (t.CHS_CK_IDI_TL_U == null && t.CHS_CK_IDI_TL_L == null)
            {
                disabled.AddRange(new[]
                {
                    "CHM_CK_A1_INSID_DI", "CHM_CK_B1_INSID_DI",
                    "CHM_CK_A2_INSID_DI", "CHM_CK_B2_INSID_DI",
                    "CHM_CK_C1_INSID_DI", "CHM_CK_C2_INSID_DI",
                });
            }

            if (t.CHS_CK_W_LIN_TL_U == null && t.CHS_CK_W_LIN_TL_L == null)
            {
                disabled.AddRange(new[] { "CHM_CHK_W_LIN_TOP_IN", "CHM_CHK_W_LIN_TOP_OUT" });
            }

            if (t.CHS_CK_W_LIN_TL_U1 == null && t.CHS_CK_W_LIN_TL_L1 == null)
            {
                disabled.AddRange(new[] { "CHM_CHK_W_LIN_TOP_LOW_IN", "CHM_CHK_W_LIN_TOP_LOW_OUT" });
            }

            if (t.CHS_CK_LIN_TL_U == null && t.CHS_CK_LIN_TL_L == null)
            {
                disabled.AddRange(new[] { "CHM_CHK_LIN_TOP_IN", "CHM_CHK_LIN_TOP_OUT" });
            }

            if (t.CHS_CK_LIN_TL_U1 == null && t.CHS_CK_LIN_TL_L1 == null)
            {
                disabled.AddRange(new[] { "CHM_CHK_LIN_TOP_LOW_IN", "CHM_CHK_LIN_TOP_LOW_OUT" });
            }

            return disabled;
        }

        public async Task<(bool success, bool wasUpdate)> SaveChockAsync(ChockSaveRequest input)
        {
            if (string.IsNullOrWhiteSpace(input.CHM_CHK_MAKER))
            {
                throw new InvalidOperationException("Enter Chock Maker");
            }

            using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync();

            // Mirrors DUP_VAL_ON_INDEX handling in the legacy SAVE trigger:
            // check whether a live row already exists for this PK.
            string? existingStatus = null;
            using (var cmd = new OracleCommand(
                "SELECT CHM_CD_CHK_PROG FROM T_CHOCK_MAST WHERE CHM_ID_CHOCK = :Id AND CHM_CHK_TYP = :Typ AND CHM_DEL_TAG = 'N'",
                conn))
            {
                cmd.Parameters.Add(new OracleParameter("Id", OracleDbType.Varchar2) { Value = input.CHM_ID_CHOCK });
                cmd.Parameters.Add(new OracleParameter("Typ", OracleDbType.Varchar2) { Value = input.CHM_CHK_TYP });
                var result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                {
                    existingStatus = result.ToString();
                }
            }

            bool isUpdate = existingStatus != null;

            if (isUpdate && existingStatus != "CNEW")
            {
                throw new InvalidOperationException("You can Modify Chock with status CNEW only");
            }

            if (isUpdate)
            {
                await UpdateChockAsync(conn, input);
            }
            else
            {
                await InsertChockAsync(conn, input);
            }

            return (true, isUpdate);
        }

        private async Task InsertChockAsync(OracleConnection conn, ChockSaveRequest input)
        {
            const string sql = @"
                INSERT INTO T_CHOCK_MAST (
                    CHM_ID_CHOCK, CHM_CHK_TYP, CHM_ROL_TYP, CHM_CD_CHK_PROG, CHM_DT_CHK_IMP,
                    CHM_CHK_MAKER, CHM_CK_A1_INSID_DI, CHM_CK_A2_INSID_DI, CHM_CK_B1_INSID_DI,
                    CHM_CK_B2_INSID_DI, CHM_CK_C1_INSID_DI, CHM_CK_C2_INSID_DI, CHM_CHK_LIN_SZ_1,
                    CHM_CHK_W_LIN_TOP_IN, CHM_CHK_W_LIN_BOTTOM_IN, CHM_CHK_W_LIN_TOP_OUT, CHM_CHK_W_LIN_BOTTOM_OUT,
                    CHM_CHK_W_LIN_TOP_UP_IN, CHM_CHK_W_LIN_TOP_LOW_IN, CHM_CHK_W_LIN_BOTTOM_UP_IN, CHM_CHK_W_LIN_BOTTOM_LOW_IN,
                    CHM_CHK_W_LIN_TOP_UP_OUT, CHM_CHK_W_LIN_TOP_LOW_OUT, CHM_CHK_W_LIN_BOTTOM_UP_OUT, CHM_CHK_W_LIN_BOTTOM_LOW_OUT,
                    CHM_CHK_LIN_TOP_IN, CHM_CHK_LIN_BOTTOM_IN, CHM_CHK_LIN_TOP_OUT, CHM_CHK_LIN_BOTTOM_OUT,
                    CHM_CHK_LIN_TOP_UP_IN, CHM_CHK_LIN_TOP_LOW_IN, CHM_CHK_LIN_BOTTOM_UP_IN, CHM_CHK_LIN_BOTTOM_LOW_IN,
                    CHM_CHK_LIN_TOP_UP_OUT, CHM_CHK_LIN_TOP_LOW_OUT, CHM_CHK_LIN_BOTTOM_UP_OUT, CHM_CHK_LIN_BOTTOM_LOW_OUT,
                    CHM_REMARKS, CHM_DEL_TAG, CHM_DT_CREATE, CHM_DT_UPDATE, CHM_ID_USER
                ) VALUES (
                    :Id, :Typ, :Typ, 'CNEW', :ImpDate,
                    :Maker, :A1, :A2, :B1, :B2, :C1, :C2, :LinSz1,
                    :WLinTopIn, :WLinBottomIn, :WLinTopOut, :WLinBottomOut,
                    :WLinTopUpIn, :WLinTopLowIn, :WLinBottomUpIn, :WLinBottomLowIn,
                    :WLinTopUpOut, :WLinTopLowOut, :WLinBottomUpOut, :WLinBottomLowOut,
                    :LinTopIn, :LinBottomIn, :LinTopOut, :LinBottomOut,
                    :LinTopUpIn, :LinTopLowIn, :LinBottomUpIn, :LinBottomLowIn,
                    :LinTopUpOut, :LinTopLowOut, :LinBottomUpOut, :LinBottomLowOut,
                    :Remarks, 'N', SYSDATE, SYSDATE, :UserId
                )";

            using var cmd = new OracleCommand(sql, conn);
            cmd.BindByName = true;
            AddSaveParameters(cmd, input);
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task UpdateChockAsync(OracleConnection conn, ChockSaveRequest input)
        {
            const string sql = @"
                UPDATE T_CHOCK_MAST SET
                    CHM_DT_CHK_IMP = :ImpDate,
                    CHM_CHK_MAKER = :Maker,
                    CHM_CK_A1_INSID_DI = :A1, CHM_CK_A2_INSID_DI = :A2,
                    CHM_CK_B1_INSID_DI = :B1, CHM_CK_B2_INSID_DI = :B2,
                    CHM_CK_C1_INSID_DI = :C1, CHM_CK_C2_INSID_DI = :C2,
                    CHM_CHK_LIN_SZ_1 = :LinSz1,
                    CHM_CHK_W_LIN_TOP_IN = :WLinTopIn, CHM_CHK_W_LIN_BOTTOM_IN = :WLinBottomIn,
                    CHM_CHK_W_LIN_TOP_OUT = :WLinTopOut, CHM_CHK_W_LIN_BOTTOM_OUT = :WLinBottomOut,
                    CHM_CHK_W_LIN_TOP_UP_IN = :WLinTopUpIn, CHM_CHK_W_LIN_TOP_LOW_IN = :WLinTopLowIn,
                    CHM_CHK_W_LIN_BOTTOM_UP_IN = :WLinBottomUpIn, CHM_CHK_W_LIN_BOTTOM_LOW_IN = :WLinBottomLowIn,
                    CHM_CHK_W_LIN_TOP_UP_OUT = :WLinTopUpOut, CHM_CHK_W_LIN_TOP_LOW_OUT = :WLinTopLowOut,
                    CHM_CHK_W_LIN_BOTTOM_UP_OUT = :WLinBottomUpOut, CHM_CHK_W_LIN_BOTTOM_LOW_OUT = :WLinBottomLowOut,
                    CHM_CHK_LIN_TOP_IN = :LinTopIn, CHM_CHK_LIN_BOTTOM_IN = :LinBottomIn,
                    CHM_CHK_LIN_TOP_OUT = :LinTopOut, CHM_CHK_LIN_BOTTOM_OUT = :LinBottomOut,
                    CHM_CHK_LIN_TOP_UP_IN = :LinTopUpIn, CHM_CHK_LIN_TOP_LOW_IN = :LinTopLowIn,
                    CHM_CHK_LIN_BOTTOM_UP_IN = :LinBottomUpIn, CHM_CHK_LIN_BOTTOM_LOW_IN = :LinBottomLowIn,
                    CHM_CHK_LIN_TOP_UP_OUT = :LinTopUpOut, CHM_CHK_LIN_TOP_LOW_OUT = :LinTopLowOut,
                    CHM_CHK_LIN_BOTTOM_UP_OUT = :LinBottomUpOut, CHM_CHK_LIN_BOTTOM_LOW_OUT = :LinBottomLowOut,
                    CHM_REMARKS = :Remarks,
                    CHM_DT_UPDATE = SYSDATE,
                    CHM_ID_USER = :UserId
                WHERE CHM_ID_CHOCK = :Id AND CHM_CHK_TYP = :Typ AND CHM_DEL_TAG = 'N'";

            using var cmd = new OracleCommand(sql, conn);
            cmd.BindByName = true;
            AddSaveParameters(cmd, input);
            await cmd.ExecuteNonQueryAsync();
        }

        private static void AddSaveParameters(OracleCommand cmd, ChockSaveRequest input)
        {
            cmd.Parameters.Add(new OracleParameter("Id", OracleDbType.Varchar2) { Value = input.CHM_ID_CHOCK });
            cmd.Parameters.Add(new OracleParameter("Typ", OracleDbType.Varchar2) { Value = input.CHM_CHK_TYP });
            cmd.Parameters.Add(new OracleParameter("ImpDate", OracleDbType.Date) { Value = ParseDate(input.CHM_DT_CHK_IMP) });
            cmd.Parameters.Add(new OracleParameter("Maker", OracleDbType.Varchar2) { Value = input.CHM_CHK_MAKER });
            cmd.Parameters.Add(new OracleParameter("A1", OracleDbType.Decimal) { Value = ToDb(input.CHM_CK_A1_INSID_DI) });
            cmd.Parameters.Add(new OracleParameter("A2", OracleDbType.Decimal) { Value = ToDb(input.CHM_CK_A2_INSID_DI) });
            cmd.Parameters.Add(new OracleParameter("B1", OracleDbType.Decimal) { Value = ToDb(input.CHM_CK_B1_INSID_DI) });
            cmd.Parameters.Add(new OracleParameter("B2", OracleDbType.Decimal) { Value = ToDb(input.CHM_CK_B2_INSID_DI) });
            cmd.Parameters.Add(new OracleParameter("C1", OracleDbType.Decimal) { Value = ToDb(input.CHM_CK_C1_INSID_DI) });
            cmd.Parameters.Add(new OracleParameter("C2", OracleDbType.Decimal) { Value = ToDb(input.CHM_CK_C2_INSID_DI) });
            cmd.Parameters.Add(new OracleParameter("LinSz1", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_LIN_SZ_1) });

            cmd.Parameters.Add(new OracleParameter("WLinTopIn", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_W_LIN_TOP_IN) });
            cmd.Parameters.Add(new OracleParameter("WLinBottomIn", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_W_LIN_BOTTOM_IN) });
            cmd.Parameters.Add(new OracleParameter("WLinTopOut", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_W_LIN_TOP_OUT) });
            cmd.Parameters.Add(new OracleParameter("WLinBottomOut", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_W_LIN_BOTTOM_OUT) });
            cmd.Parameters.Add(new OracleParameter("WLinTopUpIn", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_W_LIN_TOP_UP_IN) });
            cmd.Parameters.Add(new OracleParameter("WLinTopLowIn", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_W_LIN_TOP_LOW_IN) });
            cmd.Parameters.Add(new OracleParameter("WLinBottomUpIn", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_W_LIN_BOTTOM_UP_IN) });
            cmd.Parameters.Add(new OracleParameter("WLinBottomLowIn", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_W_LIN_BOTTOM_LOW_IN) });
            cmd.Parameters.Add(new OracleParameter("WLinTopUpOut", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_W_LIN_TOP_UP_OUT) });
            cmd.Parameters.Add(new OracleParameter("WLinTopLowOut", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_W_LIN_TOP_LOW_OUT) });
            cmd.Parameters.Add(new OracleParameter("WLinBottomUpOut", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_W_LIN_BOTTOM_UP_OUT) });
            cmd.Parameters.Add(new OracleParameter("WLinBottomLowOut", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_W_LIN_BOTTOM_LOW_OUT) });

            cmd.Parameters.Add(new OracleParameter("LinTopIn", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_LIN_TOP_IN) });
            cmd.Parameters.Add(new OracleParameter("LinBottomIn", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_LIN_BOTTOM_IN) });
            cmd.Parameters.Add(new OracleParameter("LinTopOut", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_LIN_TOP_OUT) });
            cmd.Parameters.Add(new OracleParameter("LinBottomOut", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_LIN_BOTTOM_OUT) });
            cmd.Parameters.Add(new OracleParameter("LinTopUpIn", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_LIN_TOP_UP_IN) });
            cmd.Parameters.Add(new OracleParameter("LinTopLowIn", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_LIN_TOP_LOW_IN) });
            cmd.Parameters.Add(new OracleParameter("LinBottomUpIn", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_LIN_BOTTOM_UP_IN) });
            cmd.Parameters.Add(new OracleParameter("LinBottomLowIn", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_LIN_BOTTOM_LOW_IN) });
            cmd.Parameters.Add(new OracleParameter("LinTopUpOut", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_LIN_TOP_UP_OUT) });
            cmd.Parameters.Add(new OracleParameter("LinTopLowOut", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_LIN_TOP_LOW_OUT) });
            cmd.Parameters.Add(new OracleParameter("LinBottomUpOut", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_LIN_BOTTOM_UP_OUT) });
            cmd.Parameters.Add(new OracleParameter("LinBottomLowOut", OracleDbType.Decimal) { Value = ToDb(input.CHM_CHK_LIN_BOTTOM_LOW_OUT) });

            cmd.Parameters.Add(new OracleParameter("Remarks", OracleDbType.Varchar2) { Value = (object?)input.CHM_REMARKS ?? DBNull.Value });
            cmd.Parameters.Add(new OracleParameter("UserId", OracleDbType.Varchar2) { Value = "APPUSER" });
        }

        private static object ToDb(decimal? value) => value.HasValue ? value.Value : DBNull.Value;

        private static object ParseDate(string? yyyyMmDd)
        {
            if (string.IsNullOrWhiteSpace(yyyyMmDd)) return DBNull.Value;
            return DateTime.TryParse(yyyyMmDd, out var dt) ? dt : DBNull.Value;
        }

        private static decimal? GetDecimal(OracleDataReader reader, string column)
        {
            var value = reader[column];
            return value == DBNull.Value ? null : Convert.ToDecimal(value);
        }

        private static string? GetString(OracleDataReader reader, string column)
        {
            var value = reader[column];
            return value == DBNull.Value ? null : value.ToString();
        }

        private static ChockRecordDto ReadChockRecord(OracleDataReader reader)
        {
            return new ChockRecordDto
            {
                CHM_CHK_TYP = GetString(reader, "CHM_CHK_TYP") ?? string.Empty,
                CHM_ID_CHOCK = GetString(reader, "CHM_ID_CHOCK") ?? string.Empty,
                CHM_CD_CHK_PROG = GetString(reader, "CHM_CD_CHK_PROG") ?? string.Empty,
                CHM_DT_CHK_IMP = FormatDate(reader["CHM_DT_CHK_IMP"]),
                CHM_CHK_MAKER = GetString(reader, "CHM_CHK_MAKER") ?? string.Empty,
                CHM_REMARKS = GetString(reader, "CHM_REMARKS") ?? string.Empty,

                CHM_CK_A1_INSID_DI = GetDecimal(reader, "CHM_CK_A1_INSID_DI"),
                CHM_CK_A2_INSID_DI = GetDecimal(reader, "CHM_CK_A2_INSID_DI"),
                CHM_CK_B1_INSID_DI = GetDecimal(reader, "CHM_CK_B1_INSID_DI"),
                CHM_CK_B2_INSID_DI = GetDecimal(reader, "CHM_CK_B2_INSID_DI"),
                CHM_CK_C1_INSID_DI = GetDecimal(reader, "CHM_CK_C1_INSID_DI"),
                CHM_CK_C2_INSID_DI = GetDecimal(reader, "CHM_CK_C2_INSID_DI"),
                CHM_CHK_LIN_SZ_1 = GetDecimal(reader, "CHM_CHK_LIN_SZ_1"),

                CHM_CHK_W_LIN_TOP_IN = GetDecimal(reader, "CHM_CHK_W_LIN_TOP_IN"),
                CHM_CHK_W_LIN_BOTTOM_IN = GetDecimal(reader, "CHM_CHK_W_LIN_BOTTOM_IN"),
                CHM_CHK_W_LIN_TOP_OUT = GetDecimal(reader, "CHM_CHK_W_LIN_TOP_OUT"),
                CHM_CHK_W_LIN_BOTTOM_OUT = GetDecimal(reader, "CHM_CHK_W_LIN_BOTTOM_OUT"),
                CHM_CHK_W_LIN_TOP_UP_IN = GetDecimal(reader, "CHM_CHK_W_LIN_TOP_UP_IN"),
                CHM_CHK_W_LIN_TOP_LOW_IN = GetDecimal(reader, "CHM_CHK_W_LIN_TOP_LOW_IN"),
                CHM_CHK_W_LIN_BOTTOM_UP_IN = GetDecimal(reader, "CHM_CHK_W_LIN_BOTTOM_UP_IN"),
                CHM_CHK_W_LIN_BOTTOM_LOW_IN = GetDecimal(reader, "CHM_CHK_W_LIN_BOTTOM_LOW_IN"),
                CHM_CHK_W_LIN_TOP_UP_OUT = GetDecimal(reader, "CHM_CHK_W_LIN_TOP_UP_OUT"),
                CHM_CHK_W_LIN_TOP_LOW_OUT = GetDecimal(reader, "CHM_CHK_W_LIN_TOP_LOW_OUT"),
                CHM_CHK_W_LIN_BOTTOM_UP_OUT = GetDecimal(reader, "CHM_CHK_W_LIN_BOTTOM_UP_OUT"),
                CHM_CHK_W_LIN_BOTTOM_LOW_OUT = GetDecimal(reader, "CHM_CHK_W_LIN_BOTTOM_LOW_OUT"),

                CHM_CHK_LIN_TOP_IN = GetDecimal(reader, "CHM_CHK_LIN_TOP_IN"),
                CHM_CHK_LIN_BOTTOM_IN = GetDecimal(reader, "CHM_CHK_LIN_BOTTOM_IN"),
                CHM_CHK_LIN_TOP_OUT = GetDecimal(reader, "CHM_CHK_LIN_TOP_OUT"),
                CHM_CHK_LIN_BOTTOM_OUT = GetDecimal(reader, "CHM_CHK_LIN_BOTTOM_OUT"),
                CHM_CHK_LIN_TOP_UP_IN = GetDecimal(reader, "CHM_CHK_LIN_TOP_UP_IN"),
                CHM_CHK_LIN_TOP_LOW_IN = GetDecimal(reader, "CHM_CHK_LIN_TOP_LOW_IN"),
                CHM_CHK_LIN_BOTTOM_UP_IN = GetDecimal(reader, "CHM_CHK_LIN_BOTTOM_UP_IN"),
                CHM_CHK_LIN_BOTTOM_LOW_IN = GetDecimal(reader, "CHM_CHK_LIN_BOTTOM_LOW_IN"),
                CHM_CHK_LIN_TOP_UP_OUT = GetDecimal(reader, "CHM_CHK_LIN_TOP_UP_OUT"),
                CHM_CHK_LIN_TOP_LOW_OUT = GetDecimal(reader, "CHM_CHK_LIN_TOP_LOW_OUT"),
                CHM_CHK_LIN_BOTTOM_UP_OUT = GetDecimal(reader, "CHM_CHK_LIN_BOTTOM_UP_OUT"),
                CHM_CHK_LIN_BOTTOM_LOW_OUT = GetDecimal(reader, "CHM_CHK_LIN_BOTTOM_LOW_OUT"),
            };
        }

        private static string? FormatDate(object value)
        {
            if (value == DBNull.Value) return null;
            return Convert.ToDateTime(value).ToString("yyyy-MM-dd");
        }
    }
}
