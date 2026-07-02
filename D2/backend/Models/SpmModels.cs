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
}
