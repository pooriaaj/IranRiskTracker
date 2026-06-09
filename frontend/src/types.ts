export type DashboardTopContributor = {
  indicatorKey: string
  indicatorName: string
  category: string
  weightedContribution: number
  baseScore: number
  severityAdjustedBaseScore: number
  explanation: string
}

export type DashboardSummaryDto = {
  score: number
  scorePercent: number
  level: string
  previousScore?: number | null
  scoreChange: number
  scoreTrend: string
  timestamp: string
  summary: string
  topContributors: DashboardTopContributor[]
}
