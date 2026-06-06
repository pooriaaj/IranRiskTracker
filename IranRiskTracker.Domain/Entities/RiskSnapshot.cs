using System;
using System.Collections.Generic;
using IranRiskTracker.Domain.ValueObjects;

namespace IranRiskTracker.Domain.Entities
{
    /// <summary>
    /// Immutable point-in-time risk record produced by the scoring engine.
    /// </summary>
    public class RiskSnapshot
    {
        public Guid Id { get; init; }
        public decimal RiskPercent { get; init; }
        public DateTime ComputedAt { get; init; }
        public string ModelVersion { get; init; } = string.Empty;
        public Guid? TriggeringEventId { get; init; }

        public ICollection<IndicatorScore> IndicatorScores { get; init; } = new List<IndicatorScore>();
        public ICollection<ModelDecision> Decisions { get; init; } = new List<ModelDecision>();
    }
}
