using IranRiskTracker.Domain.Enums;

namespace IranRiskTracker.Application.DTOs
{
    public class DashboardTopContributorDto
    {
        public string IndicatorKey { get; set; } = string.Empty;
        public string IndicatorName { get; set; } = string.Empty;
        public EventCategory Category { get; set; }
        public double WeightedContribution { get; set; }
        public double BaseScore { get; set; }
        public double SeverityAdjustedBaseScore { get; set; }
        public string Explanation { get; set; } = string.Empty;
    }
}
