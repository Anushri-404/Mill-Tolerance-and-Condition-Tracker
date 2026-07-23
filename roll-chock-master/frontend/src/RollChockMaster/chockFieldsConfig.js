// chockFieldsConfig.js
// Structured to mirror the legacy Oracle Forms "Roll Chock Master" screen's
// actual table layout (Inner Chock Diameter / Chock Width panels), not just
// a flat list of fields.

// Inner Chock Diameter: 3 rows, each with an A(LZ) and B(NLZ) value.
// T_CHOCK_MAST has 6 diameter columns (A1,A2,B1,B2,C1,C2) for these 3 rows.
export const DIAMETER_ROWS = [
  { row: 1, aField: "CHM_CK_A1_INSID_DI", bField: "CHM_CK_B1_INSID_DI" },
  { row: 2, aField: "CHM_CK_A2_INSID_DI", bField: "CHM_CK_B2_INSID_DI" },
  { row: 3, aField: "CHM_CK_C1_INSID_DI", bField: "CHM_CK_C2_INSID_DI" },
];

// Shared row shape for both "Chock Width without Liner" (CHM_CHK_W_LIN_*)
// and "Chock Width with Liner" (CHM_CHK_LIN_*) panels — same 6 rows,
// same Inboard/Outboard columns, different column prefix.
export const WIDTH_ROWS = [
  { label: "Top", suffix: "TOP", hasTolerance: true },
  { label: "Bottom", suffix: "BOTTOM", hasTolerance: false },
  { label: "Top Upper", suffix: "TOP_UP", hasTolerance: false },
  { label: "Top Lower", suffix: "TOP_LOW", hasTolerance: true },
  { label: "Bottom Upper", suffix: "BOTTOM_UP", hasTolerance: false },
  { label: "Bottom Lower", suffix: "BOTTOM_LOW", hasTolerance: false },
];

export const widthField = (prefix, suffix, side) => `${prefix}${suffix}_${side}`;

export const WITHOUT_LINER_PREFIX = "CHM_CHK_W_LIN_";
export const WITH_LINER_PREFIX = "CHM_CHK_LIN_";
