using IranRiskTracker.Domain.Enums;
using System;

namespace IranRiskTracker.Application.DTOs
{
    /// <summary>
    /// Describes how a single indicator contributed to the overall risk score.
    /// </summary>
    public class IndicatorRiskContributionDto
    {
        public string IndicatorKey { get; set; } = string.Empty;
        public string IndicatorName { get; set; } = string.Empty;
        public EventCategory Category { get; set; }
        public decimal Weight { get; set; }
        public int MatchingHistoricalEventCount { get; set; }
        public double BaseScore { get; set; }
        public double WeightedContribution { get; set; }
        public string Explanation { get; set; } = string.Empty;
    }
}
