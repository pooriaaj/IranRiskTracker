using System;
using System.Collections.Generic;
using IranRiskTracker.Domain.Enums;

namespace IranRiskTracker.Domain.Entities
{
    /// <summary>
    /// A tracked risk dimension. Weight (0–1) determines proportional contribution to RiskPercent.
    /// DirectionMultiplier is +1 or -1 to handle indicators where higher values reduce overall risk.
    /// </summary>
    public class Indicator
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Key { get; init; } = string.Empty;
        public EventCategory Category { get; init; }
        public decimal Weight { get; init; }
        public int DirectionMultiplier { get; init; }
        public string? Notes { get; init; }

        public ICollection<ScoringRule> Rules { get; init; } = new List<ScoringRule>();
    }
}
