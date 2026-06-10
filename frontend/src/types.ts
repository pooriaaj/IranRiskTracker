export type DashboardTopContributor = {
  indicatorKey: string
  indicatorName: string
  category: number | string
  weightedContribution: number
  baseScore: number
  severityAdjustedBaseScore: number
  explanation: string
}

export type DashboardSummaryDto = {
  score: number
  scorePercent: number
  level: number | string
  previousScore?: number | null
  scoreChange: number
  scoreTrend: string
  timestamp: string
  summary: string
  topContributors: DashboardTopContributor[]
}

export type LiveEventDto = {
  id: string
  title: string
  rawContent: string
  sourceName: string
  sourceUrl: string
  sourceHandle: string
  ownerNotes: string
  occurredAt: string
  urgency: number | string
  category: number | string
}

export type LiveEventCreateRequest = {
  title: string
  rawContent: string
  sourceName: string
  sourceUrl: string
  ownerNotes: string
  occurredAt: string
  urgency: number
  category: number
}
