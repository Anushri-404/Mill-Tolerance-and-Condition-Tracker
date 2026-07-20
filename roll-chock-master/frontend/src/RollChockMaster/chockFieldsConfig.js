
export const DIAMETER_ROWS = [
  { row: 1, aField: "CHM_CK_A1_INSID_DI", bField: "CHM_CK_B1_INSID_DI" },
  { row: 2, aField: "CHM_CK_A2_INSID_DI", bField: "CHM_CK_B2_INSID_DI" },
  { row: 3, aField: "CHM_CK_C1_INSID_DI", bField: "CHM_CK_C2_INSID_DI" },
];

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
