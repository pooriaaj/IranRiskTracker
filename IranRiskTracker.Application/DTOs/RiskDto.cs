using System;
using IranRiskTracker.Domain.Enums;
using System.Collections.Generic;

namespace IranRiskTracker.Application.DTOs
{
    /// <summary>
    /// DTO representing a risk snapshot returned by the API.
    /// </summary>
    public class RiskDto
    {
        public DateTime Timestamp { get; set; }
        public RiskLevel Level { get; set; }
        public double Score { get; set; }
        public string Summary { get; set; } = string.Empty;
        public IReadOnlyCollection<IndicatorRiskContributionDto> Contributions { get; set; } = Array.Empty<IndicatorRiskContributionDto>();
    }
}
