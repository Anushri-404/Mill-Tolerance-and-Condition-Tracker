namespace RollChockBackend.Models
{
    
    public class ChockSaveRequest
    {
        public string CHM_CHK_TYP { get; set; } = string.Empty;
        public string CHM_ID_CHOCK { get; set; } = string.Empty;
        public string CHM_CD_CHK_PROG { get; set; } = string.Empty;
        public string? CHM_DT_CHK_IMP { get; set; } // yyyy-MM-dd from <input type="date">
        public string CHM_CHK_MAKER { get; set; } = string.Empty;
        public string CHM_REMARKS { get; set; } = string.Empty;

        public decimal? CHM_CK_A1_INSID_DI { get; set; }
        public decimal? CHM_CK_A2_INSID_DI { get; set; }
        public decimal? CHM_CK_B1_INSID_DI { get; set; }
        public decimal? CHM_CK_B2_INSID_DI { get; set; }
        public decimal? CHM_CK_C1_INSID_DI { get; set; }
        public decimal? CHM_CK_C2_INSID_DI { get; set; }
        public decimal? CHM_CHK_LIN_SZ_1 { get; set; }

        public decimal? CHM_CHK_W_LIN_TOP_IN { get; set; }
        public decimal? CHM_CHK_W_LIN_BOTTOM_IN { get; set; }
        public decimal? CHM_CHK_W_LIN_TOP_OUT { get; set; }
        public decimal? CHM_CHK_W_LIN_BOTTOM_OUT { get; set; }
        public decimal? CHM_CHK_W_LIN_TOP_UP_IN { get; set; }
        public decimal? CHM_CHK_W_LIN_TOP_LOW_IN { get; set; }
        public decimal? CHM_CHK_W_LIN_BOTTOM_UP_IN { get; set; }
        public decimal? CHM_CHK_W_LIN_BOTTOM_LOW_IN { get; set; }
        public decimal? CHM_CHK_W_LIN_TOP_UP_OUT { get; set; }
        public decimal? CHM_CHK_W_LIN_TOP_LOW_OUT { get; set; }
        public decimal? CHM_CHK_W_LIN_BOTTOM_UP_OUT { get; set; }
        public decimal? CHM_CHK_W_LIN_BOTTOM_LOW_OUT { get; set; }

        public decimal? CHM_CHK_LIN_TOP_IN { get; set; }
        public decimal? CHM_CHK_LIN_BOTTOM_IN { get; set; }
        public decimal? CHM_CHK_LIN_TOP_OUT { get; set; }
        public decimal? CHM_CHK_LIN_BOTTOM_OUT { get; set; }
        public decimal? CHM_CHK_LIN_TOP_UP_IN { get; set; }
        public decimal? CHM_CHK_LIN_TOP_LOW_IN { get; set; }
        public decimal? CHM_CHK_LIN_BOTTOM_UP_IN { get; set; }
        public decimal? CHM_CHK_LIN_BOTTOM_LOW_IN { get; set; }
        public decimal? CHM_CHK_LIN_TOP_UP_OUT { get; set; }
        public decimal? CHM_CHK_LIN_TOP_LOW_OUT { get; set; }
        public decimal? CHM_CHK_LIN_BOTTOM_UP_OUT { get; set; }
        public decimal? CHM_CHK_LIN_BOTTOM_LOW_OUT { get; set; }
    }

    public class ChockRecordDto : ChockSaveRequest
    {
    }

    public class ChockToleranceDto
    {
        public decimal? CHS_CK_IDI_TL_U { get; set; }
        public decimal? CHS_CK_IDI_TL_L { get; set; }
        public decimal? CHS_CK_END_TL_U { get; set; }
        public decimal? CHS_CK_END_TL_L { get; set; }
        public decimal? CHS_CK_W_LIN_TL_U { get; set; }
        public decimal? CHS_CK_W_LIN_TL_L { get; set; }
        public decimal? CHS_CK_W_LIN_TL_U1 { get; set; }
        public decimal? CHS_CK_W_LIN_TL_L1 { get; set; }
        public decimal? CHS_CK_LIN_TL_U { get; set; }
        public decimal? CHS_CK_LIN_TL_L { get; set; }
        public decimal? CHS_CK_LIN_TL_U1 { get; set; }
        public decimal? CHS_CK_LIN_TL_L1 { get; set; }
    }

    public class ChockQueryResponse
    {
        public bool Found { get; set; }
        public ChockRecordDto? Chock { get; set; }
        public string? StatusDesc { get; set; }
        public ChockToleranceDto? Tolerance { get; set; }
    }

    public class CodeDto
    {
        public string CodeValue { get; set; } = string.Empty;
        public string CodeDesc { get; set; } = string.Empty;
    }

    public class ChockLookupsDto
    {
        public List<string> ChockTypes { get; set; } = new();
        public List<CodeDto> ChockMakers { get; set; } = new();
    }
}
