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
