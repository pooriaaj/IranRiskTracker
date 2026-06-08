using System;
using System.Collections.Generic;
using IranRiskTracker.Domain.Enums;

namespace IranRiskTracker.Application.DTOs
{
    public class DashboardSummaryDto
    {
        public double Score { get; set; }
        public int ScorePercent { get; set; }
        public RiskLevel Level { get; set; }
        public double? PreviousScore { get; set; }
        public double ScoreChange { get; set; }
        public string ScoreTrend { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Summary { get; set; } = string.Empty;
        public IReadOnlyCollection<DashboardTopContributorDto> TopContributors { get; set; } = Array.Empty<DashboardTopContributorDto>();
    }
}
