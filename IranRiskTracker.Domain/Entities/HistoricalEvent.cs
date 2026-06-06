using System;
using IranRiskTracker.Domain.Enums;

namespace IranRiskTracker.Domain.Entities
{
    /// <summary>
    /// Historical events used to seed known past incidents.
    /// Child of EventCategory (conceptual). Kept simple for Phase 1.
    /// </summary>
    public class HistoricalEvent
    {
        public Guid Id { get; set; }
        public DateTime OccurredAt { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Details { get; set; }
        public RiskLevel Severity { get; set; }

        public HistoricalEvent() { }
    }
}
