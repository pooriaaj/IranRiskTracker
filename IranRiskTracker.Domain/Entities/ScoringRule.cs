using System;
using IranRiskTracker.Domain.Enums;

namespace IranRiskTracker.Domain.Entities
{
    /// <summary>
    /// Conditional rule attached to an indicator to govern how events affect it.
    /// </summary>
    public class ScoringRule
    {
        public Guid Id { get; init; }
        public Guid IndicatorId { get; init; }
        public EventCategory MatchCategory { get; init; }
        public RuleType RuleType { get; init; }
        public decimal WeightOverride { get; init; }
        public string Justification { get; init; } = string.Empty;

        public Indicator? Indicator { get; init; }
    }
}
