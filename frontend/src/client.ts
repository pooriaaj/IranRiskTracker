import type { DashboardSummaryDto } from './types'

const base = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000'

export async function getDashboardSummary(): Promise<DashboardSummaryDto> {
  const res = await fetch(`${base}/api/dashboard/summary`)
  if (!res.ok) throw new Error(`API error ${res.status}`)
  return res.json()
}
