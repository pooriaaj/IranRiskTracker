using IranRiskTracker.Domain.Enums;
using System;

namespace IranRiskTracker.Application.DTOs
{
    public class IndicatorRiskContributionDto
    {
        public string IndicatorKey { get; set; } = string.Empty;
        public string IndicatorName { get; set; } = string.Empty;
        public EventCategory Category { get; set; }
        public decimal Weight { get; set; }
        public int MatchingHistoricalEventCount { get; set; }
        public int MatchingLiveEventCount { get; set; }
        public double BaseScore { get; set; }
        public double HistoricalBaseScore { get; set; }
        public double LiveBaseScore { get; set; }
        public double CorroborationMultiplier { get; set; }
        public int DistinctLiveSourceCount { get; set; }
        public double WeightedContribution { get; set; }
        public string Explanation { get; set; } = string.Empty;
        public IReadOnlyCollection<LiveSignalContributionDto> LiveSignals { get; set; } = Array.Empty<LiveSignalContributionDto>();
    }
}
