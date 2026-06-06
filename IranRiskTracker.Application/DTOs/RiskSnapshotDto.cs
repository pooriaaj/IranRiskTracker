using System;
using System.Collections.Generic;

namespace IranRiskTracker.Application.DTOs
{
    /// <summary>
    /// Lightweight snapshot DTO used by the API while the full scoring model is not yet implemented.
    /// </summary>
    public class RiskSnapshotDto
    {
        public Guid Id { get; set; }
        public decimal RiskPercent { get; set; }
        public DateTime ComputedAt { get; set; }
        public string ModelVersion { get; set; } = string.Empty;
        public IEnumerable<object> IndicatorScores { get; set; } = Array.Empty<object>();
    }
}
