// chockLookupService.js
//
// Mock stand-ins for the T_CODES-backed dropdowns (CD_TYPE='C0032' -> Chock
// Type, CD_TYPE='CHKMK' -> Chock Maker, CD_TYPE='C0030' -> Status description).
// Swap these for real fetch() calls once your backend exposes chock endpoints.

const MOCK_CHOCK_TYPES = ["CK-A", "CK-B", "CK-C"];
const MOCK_CHOCK_MAKERS = ["MAKER1", "MAKER2", "MAKER3"];
const MOCK_STATUS_DESC = { CNEW: "New - Not Yet Processed", CACT: "Active" };

export async function fetchChockLookups() {
  // TODO: replace with real GET /api/spm/codes/{type} calls
  return {
    chockType: MOCK_CHOCK_TYPES,
    chockMaker: MOCK_CHOCK_MAKERS,
    statusDesc: MOCK_STATUS_DESC,
  };
}

// Mirrors the EXECUTE query trigger in C1KKS002_program_logic.txt
export async function fetchExistingChock(chockId, chockType) {
  // TODO: replace with real GET /api/spm/chock/{chockId}/{chockType}
  return null; // null = not found, caller treats this as "new record"
}

export async function saveChock(payload) {
  // TODO: replace with real POST/PUT /api/spm/chock
  console.log("Saving chock (mock):", payload);
  return { success: true };
}
