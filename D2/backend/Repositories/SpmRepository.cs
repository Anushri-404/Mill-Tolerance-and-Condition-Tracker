using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using backend.Models;

namespace backend.Repositories
{
    public class SpmRepository : ISpmRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<SpmRepository> _logger;
        private readonly bool _useMock;

        // Mock Tables
        private static readonly List<MockCode> MockCodes = new()
        {
            new("OBS", 1, "1", "Normal Condition Found"),
            new("OBS", 1, "10", "Scratch mark on roll"),
            new("AFP", 1, "10", "C"),
            new("OBS", 1, "11", "Pitting on roll"),
            new("AFP", 1, "11", "DS"),
            new("OBS", 1, "12", "Lining wear out"),
            new("AFP", 1, "12", "OS"),
            new("OBS", 1, "13", "Others"),
            new("AFP", 1, "13", "QDS"),
            new("AFP", 1, "14", "QOS"),
            new("AFP", 1, "15", "AFP6"),
            new("AFP", 1, "16", "AFP7"),
            new("AFP", 1, "17", "NA"),
            new("OBS", 1, "2", "Metal Pick-Up Found"),
            new("OBS", 1, "3", "Longitudinal/ Transverse Crack"),
            new("OBS", 1, "4", "Roll Crack/Damage"),
            new("OBS", 1, "5", "Grease on Roll, Neck"),
            new("OBS", 1, "6", "Roll discolouration"),
            new("OBS", 1, "7", "Peel Off"),
            new("OBS", 1, "8", "Dirt Pick-up on Rolls"),
            new("OBS", 1, "9", "Ovality of roll")
        };

        private static readonly List<MockEquipment> MockEquipmentList = new()
        {
            new(1, "PROCESS SECTION", "Squeeze Roll", "EL20051", "Top Roll-10", "250", "1830", "Neoprene", "Top", "20", "CBM", "NA"),
            new(1, "PROCESS SECTION", "Squeeze Roll", "EL20052", "Bottom Roll-10", "250", "1830", "Neoprene", "Bottom", "10", "CBM", "NA"),
            new(1, "PROCESS SECTION", "Squeeze Roll", "EL20053", "Top Roll-11", "250", "1830", "Neoprene", "Top", "50", "CBM", "NA"),
            new(1, "PROCESS SECTION", "Squeeze Roll", "EL20054", "Bottom Roll-11", "250", "1830", "Neoprene", "Bottom", "40", "CBM", "NA"),
            new(1, "PROCESS SECTION", "Squeeze Roll", "EL20055", "Top Roll-12", "250", "1830", "Neoprene", "Top", "30", "TBM", "4 YEARS"),
            new(1, "PROCESS SECTION", "Squeeze Roll", "EL20056", "Bottom Roll-12", "250", "1830", "Neoprene", "Bottom", "20", "TBM", "4 YEARS"),
            new(1, "EXIT SECTION", "Edge Wiper", "EL20057", "Top Idler Roll-1", "115", "1600", "Mc-Nylon", "Top", "20", "CBM", "NA"),
            new(1, "EXIT SECTION", "Edge Wiper", "EL20058", "Bottom Idler Roll-1", "115", "1600", "Mc-Nylon", "Bottom", "30", "CBM", "NA"),
            new(1, "EXIT SECTION", "Edge Wiper", "EL20059", "Top Idler Roll-2", "115", "1600", "Mc-Nylon", "Top", "10", "CBM", "NA"),
            new(1, "EXIT SECTION", "Edge Wiper", "EL20060", "Guide Plate", "NA", "NA", "Bakellite Sheet", "Bottom", "50", "CBM", "NA"),
            new(1, "EXIT SECTION", "Hot Air Dryer", "EL20061", "Bottom External Idler Roll-1", "115", "1600", "Mc-Nylon", "Bottom", "10", "CBM", "NA"),
            new(1, "ENTRY SECTION", "Uncoiler", "EL20001", "Hold Down Roll", "200", "600", "Polyurathene", "Top", "20", "CBM/TBM", "2 months"),
            new(1, "ENTRY SECTION", "Uncoiler", "EL20002", "Peeler Table", "NA", "NA", "Mild Steel", "Bottom", "30", "CBM/TBM", "2 months"),
            new(1, "ENTRY SECTION", "Idler Roll Before Pinch Roll-1", "EL20003", "Idler Roll", "70", "1600", "Metallic", "Bottom", "30", "CBM/TBM", "2 months"),
            new(1, "ENTRY SECTION", "Pinch Roll-1", "EL20004", "Pinch Roll-1 Top", "350", "1450", "Hardened Metallic", "Top", "30", "CBM/TBM", "2 months"),
            new(1, "ENTRY SECTION", "Pinch Roll-2", "EL20005", "Pinch Roll-1 Bottom", "200", "1450", "Hardened Metallic", "Bottom", "50", "CBM/TBM", "2 months")
        };

        private static readonly List<MockObservation> MockObservations = new();
        private static int _nextObsId = 33; // Starts after seed ID 32

        public SpmRepository(IConfiguration configuration, ILogger<SpmRepository> logger)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("OracleConnection") ?? string.Empty;
            _useMock = string.IsNullOrWhiteSpace(_connectionString);

            if (_useMock)
            {
                _logger.LogWarning("Oracle connection string is not configured. Running in local MOCK fallback mode.");
            }
            else
            {
                _logger.LogInformation("Oracle connection string is configured. Connecting to Oracle database.");
            }
        }

        public async Task<IEnumerable<string>> GetSectionsAsync()
        {
            if (_useMock)
            {
                // Machine ID = 1 hardcoded
                return await Task.FromResult(MockEquipmentList
                    .Where(e => e.MachineId == 1)
                    .Select(e => e.Section)
                    .Distinct()
                    .OrderBy(s => s));
            }

            return await GetMasterListStringAsync("S", "1", "", "");
        }

        public async Task<IEnumerable<string>> GetEquipL1Async(string section)
        {
            if (_useMock)
            {
                return await Task.FromResult(MockEquipmentList
                    .Where(e => e.MachineId == 1 && e.Section.Equals(section, StringComparison.OrdinalIgnoreCase))
                    .Select(e => e.EquipL1)
                    .Distinct()
                    .OrderBy(e => e));
            }

            return await GetMasterListStringAsync("EL1", "1", section, "");
        }

        public async Task<IEnumerable<SpmEquipL2Dto>> GetEquipL2Async(string section, string equipL1)
        {
            if (_useMock)
            {
                return await Task.FromResult(MockEquipmentList
                    .Where(e => e.MachineId == 1 &&
                                e.Section.Equals(section, StringComparison.OrdinalIgnoreCase) &&
                                e.EquipL1.Equals(equipL1, StringComparison.OrdinalIgnoreCase))
                    .Select(e => new SpmEquipL2Dto
                    {
                        EquipIdL2 = e.EquipIdL2,
                        EquipDescL2 = e.EquipDescL2
                    })
                    .OrderBy(e => e.EquipDescL2));
            }

            return await GetMasterListEquipL2Async("EL2", "1", section, equipL1);
        }

        public async Task<SpmGreyPartsDto?> GetGreyPartsAsync(string equipL2Id)
        {
            if (_useMock)
            {
                var equip = MockEquipmentList.FirstOrDefault(e => e.EquipIdL2.Equals(equipL2Id, StringComparison.OrdinalIgnoreCase));
                if (equip == null) return null;

                return await Task.FromResult(new SpmGreyPartsDto
                {
                    EquipL2RollDia = equip.EquipL2RollDia,
                    Hardness = equip.Hardness,
                    EquipL2RollCoat = equip.EquipL2RollCoat,
                    MaintPhilosophy = equip.MaintPhilosophy,
                    ReplacementFreq = equip.ReplacementFreq,
                    EquipL2TouchPoint = equip.EquipL2TouchPoint
                });
            }

            // Note: EL2_M calls procedure with MachineID = "0" as per guidelines: ("EL2_M", 0, "", equipLevel2_ID)
            var list = await GetMasterListGreyPartsAsync("EL2_M", "0", "", equipL2Id);
            return list.FirstOrDefault();
        }

        public async Task<IEnumerable<SpmCodeDto>> GetObservationTypesAsync()
        {
            if (_useMock)
            {
                return await Task.FromResult(MockCodes
                    .Where(c => c.MachineId == 1 && c.CodeType.Equals("OBS", StringComparison.OrdinalIgnoreCase))
                    .Select(c => new SpmCodeDto { CodeId = c.CodeId, CodeDesc = c.CodeDesc })
                    .OrderBy(c => c.CodeDesc));
            }

            return await GetMasterListCodesAsync("OBS", "1", "", "");
        }

        public async Task<IEnumerable<SpmCodeDto>> GetAffectedPortionsAsync()
        {
            if (_useMock)
            {
                return await Task.FromResult(MockCodes
                    .Where(c => c.MachineId == 1 && c.CodeType.Equals("AFP", StringComparison.OrdinalIgnoreCase))
                    .Select(c => new SpmCodeDto { CodeId = c.CodeId, CodeDesc = c.CodeDesc })
                    .OrderBy(c => c.CodeDesc));
            }

            return await GetMasterListCodesAsync("AFP", "1", "", "");
        }

        public async Task<bool> SaveObservationAsync(SpmObservationInput input, string? attachmentName, string? fileExtension)
        {
            if (_useMock)
            {
                var newObs = new MockObservation
                {
                    SpmObsId = _nextObsId++,
                    EquipIdL2 = input.EquipIdL2,
                    ObsType = input.ObsType,
                    AffectedP = input.AffectedP,
                    DefDetails = input.DefDetails,
                    Attachment = !string.IsNullOrEmpty(attachmentName) ? "YES" : "NO",
                    CreatedBy = "1221", // Hardcoded per sample
                    CreatedOn = DateTime.Now,
                    DiameterNew = input.DiameterNew,
                    HardnessNew = input.HardnessNew,
                    LiningCondNew = input.LiningCondNew,
                    BearingCondNew = input.BearingCondNew,
                    BakelitePlateCondNew = input.BakelitePlateCondNew,
                    SeverityStatus = input.SeverityStatus,
                    SpAuditDate = input.SpAuditDate,
                    LastRollChangeDate = input.LastRollChangeDate,
                    LastBearGreaseDate = input.LastBearGreaseDate,
                    Extension = fileExtension ?? string.Empty
                };

                lock (MockObservations)
                {
                    MockObservations.Add(newObs);
                }

                _logger.LogInformation("Saved Mock Observation with ID: {Id}, L2: {L2}, Severity: {Sev}, Attachment: {Att}, Ext: {Ext}",
                    newObs.SpmObsId, newObs.EquipIdL2, newObs.SeverityStatus, newObs.Attachment, newObs.Extension);

                return await Task.FromResult(true);
            }

            try
            {
                using var conn = new OracleConnection(_connectionString);
                await conn.OpenAsync();

                // Build insert command. Using standard INSERT with fallback for primary key.
                // We first check if sequence exists, or do a subquery MAX(SPM_OBS_ID) + 1.
                // To be robust, let's write SQL using COALESCE((SELECT MAX(SPM_OBS_ID) FROM TRN_SPM_OBSERVATION), 0) + 1
                string sql = @"
                    INSERT INTO TRN_SPM_OBSERVATION (
                        SPM_OBS_ID, EQUIPID_L2, OBSTYPE, AFFECTEDP, DEFDETAILS, ATTACHMENT, 
                        CREATEDBY, CREATEDON, DIAMETER_NEW, HARDNESS_NEW, LINING_COND_NEW, 
                        BEARING_COND_NEW, BAKELITE_PLATE_COND_NEW, SEVERITY_STATUS, 
                        SP_AUDIT_DATE, LAST_ROLLCHANGE_DATE, LAST_BEARGREASE_DATE, EXTENSION
                    ) VALUES (
                        (SELECT COALESCE(MAX(SPM_OBS_ID), 0) + 1 FROM TRN_SPM_OBSERVATION),
                        :EquipIdL2, :ObsType, :AffectedP, :DefDetails, :Attachment,
                        :CreatedBy, SYSDATE, :DiameterNew, :HardnessNew, :LiningCondNew,
                        :BearingCondNew, :BakelitePlateCondNew, :SeverityStatus,
                        :SpAuditDate, :LastRollChangeDate, :LastBearGreaseDate, :Extension
                    )";

                using var cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add(new OracleParameter("EquipIdL2", OracleDbType.Varchar2) { Value = (object?)input.EquipIdL2 ?? DBNull.Value });
                cmd.Parameters.Add(new OracleParameter("ObsType", OracleDbType.Varchar2) { Value = (object?)input.ObsType ?? DBNull.Value });
                cmd.Parameters.Add(new OracleParameter("AffectedP", OracleDbType.Varchar2) { Value = (object?)input.AffectedP ?? DBNull.Value });
                cmd.Parameters.Add(new OracleParameter("DefDetails", OracleDbType.Varchar2) { Value = (object?)input.DefDetails ?? DBNull.Value });
                cmd.Parameters.Add(new OracleParameter("Attachment", OracleDbType.Varchar2) { Value = !string.IsNullOrEmpty(attachmentName) ? "YES" : "NO" });
                cmd.Parameters.Add(new OracleParameter("CreatedBy", OracleDbType.Varchar2) { Value = "1221" });
                cmd.Parameters.Add(new OracleParameter("DiameterNew", OracleDbType.Varchar2) { Value = (object?)input.DiameterNew ?? DBNull.Value });
                cmd.Parameters.Add(new OracleParameter("HardnessNew", OracleDbType.Varchar2) { Value = (object?)input.HardnessNew ?? DBNull.Value });
                cmd.Parameters.Add(new OracleParameter("LiningCondNew", OracleDbType.Varchar2) { Value = (object?)input.LiningCondNew ?? DBNull.Value });
                cmd.Parameters.Add(new OracleParameter("BearingCondNew", OracleDbType.Varchar2) { Value = (object?)input.BearingCondNew ?? DBNull.Value });
                cmd.Parameters.Add(new OracleParameter("BakelitePlateCondNew", OracleDbType.Varchar2) { Value = (object?)input.BakelitePlateCondNew ?? DBNull.Value });
                cmd.Parameters.Add(new OracleParameter("SeverityStatus", OracleDbType.Varchar2) { Value = (object?)input.SeverityStatus ?? DBNull.Value });
                cmd.Parameters.Add(new OracleParameter("SpAuditDate", OracleDbType.Date) { Value = (object?)input.SpAuditDate ?? DBNull.Value });
                cmd.Parameters.Add(new OracleParameter("LastRollChangeDate", OracleDbType.Date) { Value = (object?)input.LastRollChangeDate ?? DBNull.Value });
                cmd.Parameters.Add(new OracleParameter("LastBearGreaseDate", OracleDbType.Date) { Value = (object?)input.LastBearGreaseDate ?? DBNull.Value });
                cmd.Parameters.Add(new OracleParameter("Extension", OracleDbType.Varchar2) { Value = (object?)fileExtension ?? DBNull.Value });

                int affected = await cmd.ExecuteNonQueryAsync();
                return affected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving observation to Oracle database.");
                throw;
            }
        }

        #region Helper Oracle Stored Procedure Callers

        private async Task<IEnumerable<string>> GetMasterListStringAsync(string masterType, string machineId, string section, string equipL1)
        {
            var results = new List<string>();
            try
            {
                using var conn = new OracleConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new OracleCommand("SPGET_SPM_MASTERLIST", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("P_MASTER_TYPE", OracleDbType.Varchar2).Value = masterType;
                cmd.Parameters.Add("P_MACHINEID", OracleDbType.Varchar2).Value = machineId;
                cmd.Parameters.Add("P_SECTION", OracleDbType.Varchar2).Value = section;
                cmd.Parameters.Add("P_EQUIP_L1", OracleDbType.Varchar2).Value = equipL1;

                var cv1 = new OracleParameter("CV_1", OracleDbType.RefCursor);
                cv1.Direction = ParameterDirection.InputOutput;
                cmd.Parameters.Add(cv1);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    results.Add(reader.GetValue(0)?.ToString() ?? string.Empty);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling SPGET_SPM_MASTERLIST for Type: {Type}", masterType);
                throw;
            }
            return results;
        }

        private async Task<IEnumerable<SpmCodeDto>> GetMasterListCodesAsync(string masterType, string machineId, string section, string equipL1)
        {
            var results = new List<SpmCodeDto>();
            try
            {
                using var conn = new OracleConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new OracleCommand("SPGET_SPM_MASTERLIST", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("P_MASTER_TYPE", OracleDbType.Varchar2).Value = masterType;
                cmd.Parameters.Add("P_MACHINEID", OracleDbType.Varchar2).Value = machineId;
                cmd.Parameters.Add("P_SECTION", OracleDbType.Varchar2).Value = section;
                cmd.Parameters.Add("P_EQUIP_L1", OracleDbType.Varchar2).Value = equipL1;

                var cv1 = new OracleParameter("CV_1", OracleDbType.RefCursor);
                cv1.Direction = ParameterDirection.InputOutput;
                cmd.Parameters.Add(cv1);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    results.Add(new SpmCodeDto
                    {
                        CodeId = reader["CODEID"]?.ToString() ?? string.Empty,
                        CodeDesc = reader["CODEDESC"]?.ToString() ?? string.Empty
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling SPGET_SPM_MASTERLIST for Codes Type: {Type}", masterType);
                throw;
            }
            return results;
        }

        private async Task<IEnumerable<SpmEquipL2Dto>> GetMasterListEquipL2Async(string masterType, string machineId, string section, string equipL1)
        {
            var results = new List<SpmEquipL2Dto>();
            try
            {
                using var conn = new OracleConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new OracleCommand("SPGET_SPM_MASTERLIST", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("P_MASTER_TYPE", OracleDbType.Varchar2).Value = masterType;
                cmd.Parameters.Add("P_MACHINEID", OracleDbType.Varchar2).Value = machineId;
                cmd.Parameters.Add("P_SECTION", OracleDbType.Varchar2).Value = section;
                cmd.Parameters.Add("P_EQUIP_L1", OracleDbType.Varchar2).Value = equipL1;

                var cv1 = new OracleParameter("CV_1", OracleDbType.RefCursor);
                cv1.Direction = ParameterDirection.InputOutput;
                cmd.Parameters.Add(cv1);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    results.Add(new SpmEquipL2Dto
                    {
                        EquipIdL2 = reader["EQUIPID_L2"]?.ToString() ?? string.Empty,
                        EquipDescL2 = reader["EQUIPDESC_L2"]?.ToString() ?? string.Empty
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling SPGET_SPM_MASTERLIST for L2 Type: {Type}", masterType);
                throw;
            }
            return results;
        }

        private async Task<IEnumerable<SpmGreyPartsDto>> GetMasterListGreyPartsAsync(string masterType, string machineId, string section, string equipL2Id)
        {
            var results = new List<SpmGreyPartsDto>();
            try
            {
                using var conn = new OracleConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new OracleCommand("SPGET_SPM_MASTERLIST", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("P_MASTER_TYPE", OracleDbType.Varchar2).Value = masterType;
                cmd.Parameters.Add("P_MACHINEID", OracleDbType.Varchar2).Value = machineId;
                cmd.Parameters.Add("P_SECTION", OracleDbType.Varchar2).Value = section;
                cmd.Parameters.Add("P_EQUIP_L1", OracleDbType.Varchar2).Value = equipL2Id; // L2 ID passed as L1 in procedure for EL2_M

                var cv1 = new OracleParameter("CV_1", OracleDbType.RefCursor);
                cv1.Direction = ParameterDirection.InputOutput;
                cmd.Parameters.Add(cv1);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    results.Add(new SpmGreyPartsDto
                    {
                        EquipL2RollDia = reader["EQUIP_L2_ROLLDIA"]?.ToString() ?? string.Empty,
                        Hardness = reader["HARDNESS"]?.ToString() ?? string.Empty,
                        EquipL2RollCoat = reader["EQUIP_L2_ROLLCOAT"]?.ToString() ?? string.Empty,
                        MaintPhilosophy = reader["MAINT_PHILOSOPHY"]?.ToString() ?? string.Empty,
                        ReplacementFreq = reader["REPLACEMENT_FREQ"]?.ToString() ?? string.Empty,
                        EquipL2TouchPoint = reader["EQUIP_L2_TOUCHPOINT"]?.ToString() ?? string.Empty
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling SPGET_SPM_MASTERLIST for Grey Parts");
                throw;
            }
            return results;
        }

        #endregion

        #region Mock Helper Classes

        private class MockCode
        {
            public string CodeType { get; }
            public int MachineId { get; }
            public string CodeId { get; }
            public string CodeDesc { get; }

            public MockCode(string codeType, int machineId, string codeId, string codeDesc)
            {
                CodeType = codeType;
                MachineId = machineId;
                CodeId = codeId;
                CodeDesc = codeDesc;
            }
        }

        private class MockEquipment
        {
            public int MachineId { get; }
            public string Section { get; }
            public string EquipL1 { get; }
            public string EquipIdL2 { get; }
            public string EquipDescL2 { get; }
            public string EquipL2RollDia { get; }
            public string Pitch { get; }
            public string EquipL2RollCoat { get; }
            public string EquipL2TouchPoint { get; }
            public string Hardness { get; }
            public string MaintPhilosophy { get; }
            public string ReplacementFreq { get; }

            public MockEquipment(int machineId, string section, string equipL1, string equipIdL2, string equipDescL2,
                string rollDia, string pitch, string rollCoat, string touchPoint, string hardness, string philosophy, string freq)
            {
                MachineId = machineId;
                Section = section;
                EquipL1 = equipL1;
                EquipIdL2 = equipIdL2;
                EquipDescL2 = equipDescL2;
                EquipL2RollDia = rollDia;
                Pitch = pitch;
                EquipL2RollCoat = rollCoat;
                EquipL2TouchPoint = touchPoint;
                Hardness = hardness;
                MaintPhilosophy = philosophy;
                ReplacementFreq = freq;
            }
        }

        private class MockObservation
        {
            public int SpmObsId { get; set; }
            public string EquipIdL2 { get; set; } = string.Empty;
            public string ObsType { get; set; } = string.Empty;
            public string AffectedP { get; set; } = string.Empty;
            public string DefDetails { get; set; } = string.Empty;
            public string Attachment { get; set; } = string.Empty;
            public string CreatedBy { get; set; } = string.Empty;
            public DateTime CreatedOn { get; set; }
            public string DiameterNew { get; set; } = string.Empty;
            public string HardnessNew { get; set; } = string.Empty;
            public string LiningCondNew { get; set; } = string.Empty;
            public string BearingCondNew { get; set; } = string.Empty;
            public string BakelitePlateCondNew { get; set; } = string.Empty;
            public string SeverityStatus { get; set; } = string.Empty;
            public DateTime? SpAuditDate { get; set; }
            public DateTime? LastRollChangeDate { get; set; }
            public DateTime? LastBearGreaseDate { get; set; }
            public string Extension { get; set; } = string.Empty;
        }

        #endregion
    }
}
