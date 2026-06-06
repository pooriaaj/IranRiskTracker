using System;
using IranRiskTracker.Domain.Enums;

namespace IranRiskTracker.Domain.Entities
{
    /// <summary>
    /// Records how an event moved a specific indicator.
    /// </summary>
    public class EventImpact
    {
        public Guid Id { get; init; }
        public Guid EventId { get; init; }
        public Guid IndicatorId { get; init; }
        public decimal RawDelta { get; init; }
        public decimal AdjustedDelta { get; init; }
        public string Reason { get; init; } = string.Empty;
        public SignalType SignalType { get; init; }
    }
}
