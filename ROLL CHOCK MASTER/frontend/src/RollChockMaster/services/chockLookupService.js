// chockLookupService.js
//
// Talks to the Roll Chock backend (its own ASP.NET Core API, separate
// from the SPM backend). Change API_BASE if you run the backend on a
// different port than the default used in Program.cs (5210).

const API_BASE = `${import.meta.env.VITE_API_URL}/api/chock`

export async function fetchChockLookups() {
  const res = await fetch(`${API_BASE}/lookups`)
  if (!res.ok) {
    throw new Error('Failed to load Chock Type / Chock Maker lookups')
  }
  const data = await res.json()
  // Backend returns PascalCase-ish JSON keys (ChockTypes, ChockMakers);
  // ASP.NET Core's default JSON serializer camel-cases them automatically.
  return {
    chockType: data.chockTypes ?? [],
    chockMaker: data.chockMakers ?? [], // [{ codeValue, codeDesc }]
  }
}

// Mirrors the EXECUTE-QUERY trigger in C1KKS002_program_logic.txt: fetches
// the chock master row, its status description, and its tolerance
// standards row (Lower/Upper limits) in one call.
export async function fetchExistingChock(chockId, chockType) {
  const params = new URLSearchParams({ chockId, chockType })
  const res = await fetch(`${API_BASE}/query?${params.toString()}`)
  if (!res.ok) {
    throw new Error('Query failed')
  }
  const data = await res.json()
  if (!data.found) {
    return null
  }
  return {
    record: data.chock,
    statusDesc: data.statusDesc ?? '',
    tolerance: data.tolerance ?? null,
  }
}

// Called when the Chock Type is selected (and again once a Chock ID is
// known) to drive the two "automatic" behaviors: the default status
// code/description for a new chock, and which measurement fields have no
// tolerance standard for this type and should be greyed out.
export async function fetchChockTypeConfig(chockType, chockId) {
  const params = new URLSearchParams({ chockType })
  if (chockId) params.set('chockId', chockId)
  const res = await fetch(`${API_BASE}/type-config?${params.toString()}`)
  if (!res.ok) {
    throw new Error('Failed to load configuration for this Chock Type')
  }
  const data = await res.json()
  return {
    tolerance: data.tolerance ?? null,
    disabledFields: data.disabledFields ?? [],
    defaultStatusCode: data.defaultStatusCode ?? 'CNEW',
    defaultStatusDesc: data.defaultStatusDesc ?? '',
  }
}

export async function saveChock(payload) {
  const res = await fetch(`${API_BASE}/save`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  const data = await res.json().catch(() => ({}))
  if (!res.ok) {
    throw new Error(data.message || 'Record Failed')
  }
  return data
}
