export type DashboardTopContributor = {
  IndicatorKey: string
  IndicatorName: string
  Category: string
  WeightedContribution: number
  BaseScore: number
  SeverityAdjustedBaseScore: number
  Explanation: string
}

export type DashboardSummaryDto = {
  Score: number
  ScorePercent: number
  Level: string
  PreviousScore?: number | null
  ScoreChange: number
  ScoreTrend: string
  Timestamp: string
  Summary: string
  TopContributors: DashboardTopContributor[]
}
