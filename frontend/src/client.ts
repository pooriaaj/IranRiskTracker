import type { DashboardSummaryDto } from './types'

export async function getDashboardSummary(): Promise<DashboardSummaryDto> {
  const res = await fetch('/api/dashboard/summary')
  if (!res.ok) throw new Error(`API error ${res.status}`)
  return res.json()
}
