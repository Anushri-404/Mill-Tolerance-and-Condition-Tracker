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
        private DateTime? _oracleRetryAfterUtc;
        private readonly object _oracleStateLock = new();

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

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                _logger.LogWarning("Oracle connection string is not configured. Running in local MOCK fallback mode.");
            }
            else
            {
                _logger.LogInformation("Oracle connection string is configured. Will use Oracle when reachable, mock otherwise.");
            }
        }

        // Decides, per call, whether to hit Oracle or fall back to mock data.
        // If Oracle just failed, skip retrying for 30s so a down DB doesn't
        // make every dropdown hang on a connection timeout.
        private async Task<bool> IsOracleAvailableAsync()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                return false;
            }

            lock (_oracleStateLock)
            {
                if (_oracleRetryAfterUtc.HasValue && DateTime.UtcNow < _oracleRetryAfterUtc.Value)
                {
                    return false;
                }
            }

            try
            {
                using var conn = new OracleConnection(_connectionString + ";Connection Timeout=3");
                await conn.OpenAsync();
                await conn.CloseAsync();

                lock (_oracleStateLock)
                {
                    _oracleRetryAfterUtc = null;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Oracle unreachable, falling back to mock data for this request.");
                lock (_oracleStateLock)
                {
                    _oracleRetryAfterUtc = DateTime.UtcNow.AddSeconds(30);
                }
                return false;
            }
        }

        public async Task<IEnumerable<string>> GetSectionsAsync()
        {
            if (!await IsOracleAvailableAsync())
            {
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
            if (!await IsOracleAvailableAsync())
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
            if (!await IsOracleAvailableAsync())
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
            if (!await IsOracleAvailableAsync())
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

            var list = await GetMasterListGreyPartsAsync("EL2_M", "0", "", equipL2Id);
            return list.FirstOrDefault();
        }

        public async Task<IEnumerable<SpmCodeDto>> GetObservationTypesAsync()
        {
            if (!await IsOracleAvailableAsync())
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
            if (!await IsOracleAvailableAsync())
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
            if (!await IsOracleAvailableAsync())
            {
                var newObs = new MockObservation
                {
                    SpmObsId = _nextObsId++,
                    EquipIdL2 = input.EquipIdL2,
                    SectionName = input.SectionName,
                    EquipL1Desc = input.EquipL1Desc,
                    EquipL2Desc = input.EquipL2Desc,
                    ObsType = input.ObsType,
                    AffectedP = input.AffectedP,
                    DefDetails = input.DefDetails,
                    Attachment = !string.IsNullOrEmpty(attachmentName) ? "YES" : "NO",
                    AttachmentName = attachmentName,
                    CreatedBy = "1221",
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

                string sql = @"
                    INSERT INTO TRN_SPM_OBSERVATION (
                        SPM_OBS_ID, EQUIPID_L2, SECTION_NAME, EQUIP_L1_DESC, EQUIP_L2_DESC,
                        OBSTYPE, AFFECTEDP, DEFDETAILS, ATTACHMENT, ATTACHMENT_NAME,
                        CREATEDBY, CREATEDON, DIAMETER_NEW, HARDNESS_NEW, LINING_COND_NEW,
                        BEARING_COND_NEW, BAKELITE_PLATE_COND_NEW, SEVERITY_STATUS,
                        SP_AUDIT_DATE, LAST_ROLLCHANGE_DATE, LAST_BEARGREASE_DATE, EXTENSION
                    ) VALUES (
                        (SELECT COALESCE(MAX(SPM_OBS_ID), 0) + 1 FROM TRN_SPM_OBSERVATION),
                        :EquipIdL2, :SectionName, :EquipL1Desc, :EquipL2Desc,
                        :ObsType, :AffectedP, :DefDetails, :Attachment, :AttachmentName,
                        :CreatedBy, SYSDATE, :DiameterNew, :HardnessNew, :LiningCondNew,
                        :BearingCondNew, :BakelitePlateCondNew, :SeverityStatus,
                        :SpAuditDate, :LastRollChangeDate, :LastBearGreaseDate, :Extension
                    )";

                using var cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add(new OracleParameter("EquipIdL2", OracleDbType.Varchar2) { Value = (object?)input.EquipIdL2 ?? DBNull.Value });
                cmd.Parameters.Add(new OracleParameter("SectionName", OracleDbType.Varchar2) { Value = (object?)input.SectionName ?? DBNull.Value });
                cmd.Parameters.Add(new OracleParameter("EquipL1Desc", OracleDbType.Varchar2) { Value = (object?)input.EquipL1Desc ?? DBNull.Value });
                cmd.Parameters.Add(new OracleParameter("EquipL2Desc", OracleDbType.Varchar2) { Value = (object?)input.EquipL2Desc ?? DBNull.Value });
                cmd.Parameters.Add(new OracleParameter("ObsType", OracleDbType.Varchar2) { Value = (object?)input.ObsType ?? DBNull.Value });
                cmd.Parameters.Add(new OracleParameter("AffectedP", OracleDbType.Varchar2) { Value = (object?)input.AffectedP ?? DBNull.Value });
                cmd.Parameters.Add(new OracleParameter("DefDetails", OracleDbType.Varchar2) { Value = (object?)input.DefDetails ?? DBNull.Value });
                cmd.Parameters.Add(new OracleParameter("Attachment", OracleDbType.Varchar2) { Value = !string.IsNullOrEmpty(attachmentName) ? "YES" : "NO" });
                cmd.Parameters.Add(new OracleParameter("AttachmentName", OracleDbType.Varchar2) { Value = (object?)attachmentName ?? DBNull.Value });
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

        public async Task<IEnumerable<SpmObservationReportDto>> GetObservationReportAsync(SpmReportFilter filter)
        {
            if (!await IsOracleAvailableAsync())
            {
                var query = MockObservations.AsEnumerable();

                query = query.Where(o => o.CreatedOn.Date >= filter.StartDate.Date
                                       && o.CreatedOn.Date <= filter.EndDate.Date);

                var joined = query.Select(o =>
                {
                    var equip = MockEquipmentList.FirstOrDefault(e => e.EquipIdL2 == o.EquipIdL2);
                    return new SpmObservationReportDto
                    {
                        ObservationId = o.SpmObsId,
                        Section = !string.IsNullOrEmpty(o.SectionName) ? o.SectionName : (equip?.Section ?? string.Empty),
                        EquipLv1 = !string.IsNullOrEmpty(o.EquipL1Desc) ? o.EquipL1Desc : (equip?.EquipL1 ?? string.Empty),
                        EquipLv2Id = o.EquipIdL2,
                        EquipLv2Desc = !string.IsNullOrEmpty(o.EquipL2Desc) ? o.EquipL2Desc : (equip?.EquipDescL2 ?? string.Empty),
                        Observation = o.ObsType,
                        AffectedPortion = o.AffectedP,
                        DefectDetails = o.DefDetails,
                        // Report spec: only Diameter Actual is populated from grey-parts
                        // master data here. Rollcoat / Touchpoint / Harness /
                        // Maintenance Philosophy / Replacement Frequency are Log-form-only
                        // fields and stay blank on the Report page.
                        DiameterActual = equip?.EquipL2RollDia ?? string.Empty,
                        RollcoatActual = string.Empty,
                        RollTouchpoint = string.Empty,
                        HarnessActual = string.Empty,
                        MaintenancePhilosophy = string.Empty,
                        ReplacementFrequency = string.Empty,
                        DiameterNew = o.DiameterNew,
                        HardnessNew = o.HardnessNew,
                        LiningCondNew = o.LiningCondNew,
                        BearingCondNew = o.BearingCondNew,
                        BakeliteGuideplateCond = o.BakelitePlateCondNew,
                        Status = o.SeverityStatus,
                        StripPathAuditDate = o.SpAuditDate,
                        LastRollchangeDate = o.LastRollChangeDate,
                        LastBearingGreasingDate = o.LastBearGreaseDate,
                        LoggedOn = o.CreatedOn,
                        AttachmentName = o.AttachmentName
                    };
                });

                if (!string.IsNullOrEmpty(filter.Section))
                    joined = joined.Where(r => r.Section.Equals(filter.Section, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(filter.EquipL1))
                    joined = joined.Where(r => r.EquipLv1.Equals(filter.EquipL1, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(filter.EquipL2))
                    joined = joined.Where(r => r.EquipLv2Id.Equals(filter.EquipL2, StringComparison.OrdinalIgnoreCase));

                return await Task.FromResult(joined.OrderByDescending(r => r.ObservationId));
            }

            var results = new List<SpmObservationReportDto>();
            try
            {
                using var conn = new OracleConnection(_connectionString);
                await conn.OpenAsync();

                // Requires SECTION_NAME, EQUIP_L1_DESC, EQUIP_L2_DESC, ATTACHMENT_NAME
                // columns to exist on TRN_SPM_OBSERVATION (see ALTER TABLE step).
                string sql = @"
                    SELECT SPM_OBS_ID, EQUIPID_L2, SECTION_NAME, EQUIP_L1_DESC, EQUIP_L2_DESC,
                           OBSTYPE, AFFECTEDP, DEFDETAILS, DIAMETER_NEW, HARDNESS_NEW,
                           LINING_COND_NEW, BEARING_COND_NEW, BAKELITE_PLATE_COND_NEW,
                           SEVERITY_STATUS, SP_AUDIT_DATE, LAST_ROLLCHANGE_DATE,
                           LAST_BEARGREASE_DATE, CREATEDON, ATTACHMENT_NAME
                    FROM TRN_SPM_OBSERVATION
                    WHERE CREATEDON BETWEEN :StartDate AND :EndDate
                      AND (:Section IS NULL OR SECTION_NAME = :Section)
                      AND (:EquipL1 IS NULL OR EQUIP_L1_DESC = :EquipL1)
                      AND (:EquipL2 IS NULL OR EQUIPID_L2 = :EquipL2)
                    ORDER BY SPM_OBS_ID DESC";

                using var cmd = new OracleCommand(sql, conn);
                // ODP.NET binds by POSITION by default. This query reuses :Section,
                // :EquipL1, and :EquipL2 twice each, so positional binding runs out
                // of values and throws ORA-01008. BindByName makes each named
                // placeholder reuse the same supplied parameter correctly.
                cmd.BindByName = true;
                cmd.Parameters.Add(new OracleParameter("StartDate", OracleDbType.Date) { Value = filter.StartDate });
                cmd.Parameters.Add(new OracleParameter("EndDate", OracleDbType.Date) { Value = filter.EndDate });
                cmd.Parameters.Add(new OracleParameter("Section", OracleDbType.Varchar2) { Value = (object?)filter.Section ?? DBNull.Value });
                cmd.Parameters.Add(new OracleParameter("EquipL1", OracleDbType.Varchar2) { Value = (object?)filter.EquipL1 ?? DBNull.Value });
                cmd.Parameters.Add(new OracleParameter("EquipL2", OracleDbType.Varchar2) { Value = (object?)filter.EquipL2 ?? DBNull.Value });

                var rows = new List<SpmObservationReportDto>();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        rows.Add(new SpmObservationReportDto
                        {
                            ObservationId = Convert.ToInt32(reader["SPM_OBS_ID"]),
                            EquipLv2Id = reader["EQUIPID_L2"]?.ToString() ?? string.Empty,
                            Section = reader["SECTION_NAME"]?.ToString() ?? string.Empty,
                            EquipLv1 = reader["EQUIP_L1_DESC"]?.ToString() ?? string.Empty,
                            EquipLv2Desc = reader["EQUIP_L2_DESC"]?.ToString() ?? string.Empty,
                            Observation = reader["OBSTYPE"]?.ToString() ?? string.Empty,
                            AffectedPortion = reader["AFFECTEDP"]?.ToString() ?? string.Empty,
                            DefectDetails = reader["DEFDETAILS"]?.ToString() ?? string.Empty,
                            DiameterNew = reader["DIAMETER_NEW"]?.ToString() ?? string.Empty,
                            HardnessNew = reader["HARDNESS_NEW"]?.ToString() ?? string.Empty,
                            LiningCondNew = reader["LINING_COND_NEW"]?.ToString() ?? string.Empty,
                            BearingCondNew = reader["BEARING_COND_NEW"]?.ToString() ?? string.Empty,
                            BakeliteGuideplateCond = reader["BAKELITE_PLATE_COND_NEW"]?.ToString() ?? string.Empty,
                            Status = reader["SEVERITY_STATUS"]?.ToString() ?? string.Empty,
                            StripPathAuditDate = reader["SP_AUDIT_DATE"] as DateTime?,
                            LastRollchangeDate = reader["LAST_ROLLCHANGE_DATE"] as DateTime?,
                            LastBearingGreasingDate = reader["LAST_BEARGREASE_DATE"] as DateTime?,
                            LoggedOn = Convert.ToDateTime(reader["CREATEDON"]),
                            AttachmentName = reader["ATTACHMENT_NAME"] as string
                        });
                    }
                }

                // Enrich each row with live grey-parts data via the same stored
                // proc the Log form uses for its disabled fields. Per report
                // spec: only Diameter Actual is populated here. Rollcoat /
                // Touchpoint / Harness / Maintenance Philosophy / Replacement
                // Frequency are Log-form-only fields and stay blank on the
                // Report page.
                var greyPartsCache = new Dictionary<string, SpmGreyPartsDto?>();
                foreach (var dto in rows)
                {
                    if (!greyPartsCache.TryGetValue(dto.EquipLv2Id, out var grey))
                    {
                        grey = await GetGreyPartsAsync(dto.EquipLv2Id);
                        greyPartsCache[dto.EquipLv2Id] = grey;
                    }

                    dto.DiameterActual = grey?.EquipL2RollDia ?? string.Empty;
                    dto.RollcoatActual = string.Empty;
                    dto.RollTouchpoint = string.Empty;
                    dto.HarnessActual = string.Empty;
                    dto.MaintenancePhilosophy = string.Empty;
                    dto.ReplacementFrequency = string.Empty;

                    results.Add(dto);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching observation report from Oracle database.");
                throw;
            }

            return results;
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
                cmd.Parameters.Add("P_EQUIP_L1", OracleDbType.Varchar2).Value = equipL2Id;

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
            public string SectionName { get; set; } = string.Empty;
            public string EquipL1Desc { get; set; } = string.Empty;
            public string EquipL2Desc { get; set; } = string.Empty;
            public string ObsType { get; set; } = string.Empty;
            public string AffectedP { get; set; } = string.Empty;
            public string DefDetails { get; set; } = string.Empty;
            public string Attachment { get; set; } = string.Empty;
            public string? AttachmentName { get; set; }
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