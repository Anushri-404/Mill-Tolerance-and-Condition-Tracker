-- ============================================================
-- Run this connected AS chockuser, after 02_tables_as_chockuser.sql.
-- Seeds the dropdown values and one sample tolerance-standard row
-- so the app has something to work with immediately.
-- Adjust/add rows freely later — this is just enough to test.
-- ============================================================

-- Chock Type dropdown (CD_TYPE = 'C0032')
INSERT INTO T_CODES (CD_TYPE, CD_VALUE, CD_DESC) VALUES ('C0032', 'CK-A', 'Chock Type A');
INSERT INTO T_CODES (CD_TYPE, CD_VALUE, CD_DESC) VALUES ('C0032', 'CK-B', 'Chock Type B');
INSERT INTO T_CODES (CD_TYPE, CD_VALUE, CD_DESC) VALUES ('C0032', 'CK-C', 'Chock Type C');

-- Chock Maker dropdown (CD_TYPE = 'CHKMK')
INSERT INTO T_CODES (CD_TYPE, CD_VALUE, CD_DESC) VALUES ('CHKMK', 'MAKER1', 'Maker One');
INSERT INTO T_CODES (CD_TYPE, CD_VALUE, CD_DESC) VALUES ('CHKMK', 'MAKER2', 'Maker Two');
INSERT INTO T_CODES (CD_TYPE, CD_VALUE, CD_DESC) VALUES ('CHKMK', 'MAKER3', 'Maker Three');

-- Status description lookup (CD_TYPE = 'C0030'), keyed by status code
INSERT INTO T_CODES (CD_TYPE, CD_VALUE, CD_DESC) VALUES ('C0030', 'CNEW', 'New - Not Yet Processed');
INSERT INTO T_CODES (CD_TYPE, CD_VALUE, CD_DESC) VALUES ('C0030', 'CACT', 'Active');
INSERT INTO T_CODES (CD_TYPE, CD_VALUE, CD_DESC) VALUES ('C0030', 'CSCP', 'Scrapped');

-- Sample tolerance standard: CHS_OS_DS is the first 3 characters of the
-- Chock ID (mirrors the legacy SUBSTR(CHM_ID_CHOCK,1,3) lookup). Adjust
-- 'ABC' to match the actual Chock ID prefixes you use, or add one row
-- per prefix/type combination you need.
INSERT INTO T_CHOCK_STND (
  CHS_CHK_TYP, CHS_OS_DS,
  CHS_CK_IDI_TL_U, CHS_CK_IDI_TL_L,
  CHS_CK_END_TL_U, CHS_CK_END_TL_L,
  CHS_CK_W_LIN_TL_U, CHS_CK_W_LIN_TL_L, CHS_CK_W_LIN_TL_U1, CHS_CK_W_LIN_TL_L1,
  CHS_CK_LIN_TL_U, CHS_CK_LIN_TL_L, CHS_CK_LIN_TL_U1, CHS_CK_LIN_TL_L1
) VALUES (
  'CK-A', 'ABC',
  0.50, -0.50,
  1.00, -1.00,
  0.30, -0.30, 0.20, -0.20,
  0.30, -0.30, 0.20, -0.20
);

COMMIT;
