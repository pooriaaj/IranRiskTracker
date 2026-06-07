using System;
using IranRiskTracker.Domain.Enums;

namespace IranRiskTracker.Application.DTOs
{
    public class HistoricalImpactContributionDto
    {
        public Guid EventImpactId { get; set; }
        public Guid EventId { get; set; }
        public EventType EventType { get; set; }
        public Guid IndicatorId { get; set; }
        public decimal RawDelta { get; set; }
        public decimal AdjustedDelta { get; set; }
        public string Reason { get; set; } = string.Empty;
        public SignalType SignalType { get; set; }
    }
}
