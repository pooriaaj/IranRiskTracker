using System;
using IranRiskTracker.Domain.Enums;

namespace IranRiskTracker.Domain.Entities
{
    /// <summary>
    /// Traceability record produced per evaluated event-indicator pair.
    /// </summary>
    public class ModelDecision
    {
        public Guid Id { get; init; }
        public Guid SnapshotId { get; init; }
        public Guid EventId { get; init; }
        public Guid IndicatorId { get; init; }
        public SignalType SignalType { get; init; }
        public string Reason { get; init; } = string.Empty;
        public decimal? AppliedDelta { get; init; }

        public RiskSnapshot? Snapshot { get; init; }
    }
}
