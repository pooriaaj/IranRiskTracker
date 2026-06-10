import type { DashboardSummaryDto, LiveEventDto, LiveEventCreateRequest } from './types'

export async function getDashboardSummary(): Promise<DashboardSummaryDto> {
  const res = await fetch('/api/dashboard/summary')
  if (!res.ok) throw new Error(`API error ${res.status}`)
  return res.json()
}

export async function getLiveEvents(): Promise<LiveEventDto[]> {
  const res = await fetch('/api/events/live')
  if (!res.ok) throw new Error(`API error ${res.status}`)
  return res.json()
}

export async function postLiveEvent(req: LiveEventCreateRequest): Promise<LiveEventDto> {
  const res = await fetch('/api/events/live', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(req),
  })
  if (!res.ok) {
    const msg = await res.text()
    throw new Error(msg || `API error ${res.status}`)
  }
  return res.json()
}
