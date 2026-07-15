using System;

namespace backend.Models
{
    public class SpmCodeDto
    {
        public string CodeId { get; set; } = string.Empty;
        public string CodeDesc { get; set; } = string.Empty;
    }

    public class SpmEquipL2Dto
    {
        public string EquipIdL2 { get; set; } = string.Empty;
        public string EquipDescL2 { get; set; } = string.Empty;
    }

    public class SpmGreyPartsDto
    {
        public string EquipL2RollDia { get; set; } = string.Empty;
        public string Hardness { get; set; } = string.Empty;
        public string EquipL2RollCoat { get; set; } = string.Empty;
        public string MaintPhilosophy { get; set; } = string.Empty;
        public string ReplacementFreq { get; set; } = string.Empty;
        public string EquipL2TouchPoint { get; set; } = string.Empty;
    }

    public class SpmObservationInput
    {
        public string EquipIdL2 { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public string EquipL1Desc { get; set; } = string.Empty;
        public string EquipL2Desc { get; set; } = string.Empty;
        public string ObsType { get; set; } = string.Empty;
        public string AffectedP { get; set; } = string.Empty;
        public string DefDetails { get; set; } = string.Empty;
        public string DiameterNew { get; set; } = string.Empty;
        public string HardnessNew { get; set; } = string.Empty;
        public string LiningCondNew { get; set; } = string.Empty;
        public string BearingCondNew { get; set; } = string.Empty;
        public string BakelitePlateCondNew { get; set; } = string.Empty;
        public string SeverityStatus { get; set; } = string.Empty;
        public DateTime? SpAuditDate { get; set; }
        public DateTime? LastRollChangeDate { get; set; }
        public DateTime? LastBearGreaseDate { get; set; }
    }

    public class SpmObservationReportDto
    {
        public int ObservationId { get; set; }
        public string Section { get; set; } = string.Empty;
        public string EquipLv1 { get; set; } = string.Empty;
        public string EquipLv2Id { get; set; } = string.Empty;
        public string EquipLv2Desc { get; set; } = string.Empty;
        public string Observation { get; set; } = string.Empty;
        public string AffectedPortion { get; set; } = string.Empty;
        public string DefectDetails { get; set; } = string.Empty;
        public string DiameterActual { get; set; } = string.Empty;
        public string RollcoatActual { get; set; } = string.Empty;
        public string RollTouchpoint { get; set; } = string.Empty;
        public string HarnessActual { get; set; } = string.Empty;
        public string MaintenancePhilosophy { get; set; } = string.Empty;
        public string ReplacementFrequency { get; set; } = string.Empty;
        public string DiameterNew { get; set; } = string.Empty;
        public string HardnessNew { get; set; } = string.Empty;
        public string LiningCondNew { get; set; } = string.Empty;
        public string BearingCondNew { get; set; } = string.Empty;
        public string BakeliteGuideplateCond { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? StripPathAuditDate { get; set; }
        public DateTime? LastRollchangeDate { get; set; }
        public DateTime? LastBearingGreasingDate { get; set; }
        public DateTime LoggedOn { get; set; }
        public string? AttachmentName { get; set; }
    }

    public class SpmReportFilter
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Section { get; set; }
        public string? EquipL1 { get; set; }
        public string? EquipL2 { get; set; }
    }
}