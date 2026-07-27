-- ============================================================
-- Run this connected AS chockuser (not SYSTEM/DBA).
-- Adapted from T_CHOCK_MAST.txt / T_CHOCK_STND.txt / T_CODES.txt —
-- same columns as the legacy CGL3DBA tables, but owned directly
-- by chockuser, so no cross-schema grants or public synonyms
-- are needed for the app to query them.
-- ============================================================

-- Master data: one row per physical chock.
CREATE TABLE T_CHOCK_MAST
(
  CHM_ID_CHOCK                  VARCHAR2(6 BYTE) NOT NULL,
  CHM_CHK_TYP                   VARCHAR2(10 BYTE) NOT NULL,
  CHM_ROL_TYP                   VARCHAR2(10 BYTE),
  CHM_CD_CHK_PROG                VARCHAR2(4 BYTE),
  CHM_DT_CHK_IMP                 DATE,
  CHM_CHK_MAKER                  VARCHAR2(20 BYTE),
  CHM_CK_A1_INSID_DI             NUMBER(6,2),
  CHM_CK_A2_INSID_DI             NUMBER(6,2),
  CHM_CK_B1_INSID_DI             NUMBER(6,2),
  CHM_CK_B2_INSID_DI             NUMBER(6,2),
  CHM_CK_C1_INSID_DI             NUMBER(6,2),
  CHM_CK_C2_INSID_DI             NUMBER(6,2),
  CHM_CHK_LIN_SZ_1               NUMBER(6,2),
  CHM_DT_CHK_DEL                 DATE,
  CHM_TM_CHK_DEL                 DATE,
  CHM_CHK_W_LIN_TOP_IN           NUMBER(6,2),
  CHM_CHK_W_LIN_BOTTOM_IN        NUMBER(6,2),
  CHM_CHK_W_LIN_TOP_OUT          NUMBER(6,2),
  CHM_CHK_W_LIN_BOTTOM_OUT       NUMBER(6,2),
  CHM_CHK_W_LIN_TOP_UP_IN        NUMBER(6,2),
  CHM_CHK_W_LIN_TOP_LOW_IN       NUMBER(6,2),
  CHM_CHK_W_LIN_BOTTOM_UP_IN     NUMBER(6,2),
  CHM_CHK_W_LIN_BOTTOM_LOW_IN    NUMBER(6,2),
  CHM_CHK_W_LIN_TOP_UP_OUT       NUMBER(6,2),
  CHM_CHK_W_LIN_TOP_LOW_OUT      NUMBER(6,2),
  CHM_CHK_W_LIN_BOTTOM_UP_OUT    NUMBER(6,2),
  CHM_CHK_W_LIN_BOTTOM_LOW_OUT   NUMBER(6,2),
  CHM_CHK_LIN_TOP_IN             NUMBER(6,2),
  CHM_CHK_LIN_BOTTOM_IN          NUMBER(6,2),
  CHM_CHK_LIN_TOP_OUT            NUMBER(6,2),
  CHM_CHK_LIN_BOTTOM_OUT         NUMBER(6,2),
  CHM_CHK_LIN_TOP_UP_IN          NUMBER(6,2),
  CHM_CHK_LIN_TOP_LOW_IN         NUMBER(6,2),
  CHM_CHK_LIN_BOTTOM_UP_IN       NUMBER(6,2),
  CHM_CHK_LIN_BOTTOM_LOW_IN      NUMBER(6,2),
  CHM_CHK_LIN_TOP_UP_OUT         NUMBER(6,2),
  CHM_CHK_LIN_TOP_LOW_OUT        NUMBER(6,2),
  CHM_CHK_LIN_BOTTOM_UP_OUT      NUMBER(6,2),
  CHM_CHK_LIN_BOTTOM_LOW_OUT     NUMBER(6,2),
  CHM_REMARKS                    VARCHAR2(300 BYTE),
  CHM_DEL_TAG                    VARCHAR2(1 BYTE) DEFAULT 'N',
  CHM_DT_CREATE                  DATE,
  CHM_DT_UPDATE                  DATE,
  CHM_ID_USER                    VARCHAR2(10 BYTE),
  CONSTRAINT T_CHOCK_MAST_PK PRIMARY KEY (CHM_ID_CHOCK, CHM_CHK_TYP)
);

-- Tolerance standards per chock type / OS-DS (Operator Side / Drive Side,
-- first 3 chars of chock id). Drives the disabled Lower/Upper Limit
-- fields shown next to Inner Chock Diameter, End Cover, and the two
-- Chock Width panels.
CREATE TABLE T_CHOCK_STND
(
  CHS_CHK_TYP         VARCHAR2(10 BYTE) NOT NULL,
  CHS_OS_DS           VARCHAR2(4 BYTE)  NOT NULL,
  CHS_CK_IDI_TL_U     NUMBER(6,2),
  CHS_CK_IDI_TL_L     NUMBER(6,2),
  CHS_CK_END_TL_U     NUMBER(6,2),
  CHS_CK_END_TL_L     NUMBER(6,2),
  CHS_CK_W_LIN_TL_U   NUMBER(6,2),
  CHS_CK_W_LIN_TL_L   NUMBER(6,2),
  CHS_CK_W_LIN_TL_U1  NUMBER(6,2),
  CHS_CK_W_LIN_TL_L1  NUMBER(6,2),
  CHS_CK_LIN_TL_U     NUMBER(6,2),
  CHS_CK_LIN_TL_L     NUMBER(6,2),
  CHS_CK_LIN_TL_U1    NUMBER(6,2),
  CHS_CK_LIN_TL_L1    NUMBER(6,2),
  CONSTRAINT T_CHOCK_STND_PK PRIMARY KEY (CHS_CHK_TYP, CHS_OS_DS)
);

-- Generic lookup/codes table: dropdowns and status descriptions.
-- CD_TYPE values used by this app: C0032 (Chock Type), CHKMK (Chock
-- Maker), C0030 (Status description, keyed by CD_VALUE = status code).
CREATE TABLE T_CODES
(
  CD_TYPE   VARCHAR2(5 BYTE)  NOT NULL,
  CD_VALUE  VARCHAR2(20 BYTE) NOT NULL,
  CD_DESC   VARCHAR2(70 BYTE),
  CONSTRAINT T_CODES_PK PRIMARY KEY (CD_TYPE, CD_VALUE)
);
