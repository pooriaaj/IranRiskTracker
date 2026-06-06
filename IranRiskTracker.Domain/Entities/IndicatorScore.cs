using System;

namespace IranRiskTracker.Domain.Entities
{
    /// <summary>
    /// The computed value of one indicator at snapshot time.
    /// </summary>
    public class IndicatorScore
    {
        public Guid Id { get; init; }
        public Guid SnapshotId { get; init; }
        public Guid IndicatorId { get; init; }
        public decimal RawScore { get; init; }
        public decimal WeightedScore { get; init; }
        public DateTime ComputedAt { get; init; }

        public Indicator? Indicator { get; init; }
        public RiskSnapshot? Snapshot { get; init; }
    }
}
