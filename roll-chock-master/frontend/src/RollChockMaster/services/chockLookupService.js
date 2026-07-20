
const API_BASE = 'http://localhost:5210/api/chock'

export async function fetchChockLookups() {
  const res = await fetch(`${API_BASE}/lookups`)
  if (!res.ok) {
    throw new Error('Failed to load Chock Type / Chock Maker lookups')
  }
  const data = await res.json()
  return {
    chockType: data.chockTypes ?? [],
    chockMaker: data.chockMakers ?? [], // [{ codeValue, codeDesc }]
  }
}
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
