using System;
using System.Collections.Generic;
using IranRiskTracker.Domain.Enums;

namespace IranRiskTracker.Domain.Entities
{
    /// <summary>
    /// Represents a verified past event used to calibrate the baseline risk model.
    /// HistoricalEvents are immutable baseline records.
    /// </summary>
    public class HistoricalEvent
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public DateTime OccurredAt { get; init; }
        public EventCategory Category { get; init; }
        public string? RegionTag { get; init; }
        public DateTime? VerifiedAt { get; init; }
        public bool IsBaseline { get; init; } = true;

        public ICollection<EventSource> Sources { get; init; } = new List<EventSource>();
        public ICollection<EventImpact> Impacts { get; init; } = new List<EventImpact>();
    }
}
