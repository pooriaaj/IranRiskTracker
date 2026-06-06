using System;
using System.Collections.Generic;
using IranRiskTracker.Domain.Enums;

namespace IranRiskTracker.Domain.Entities
{
    /// <summary>
    /// Represents a newly ingested event after the system goes live.
    /// </summary>
    public class LiveEvent
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? RawContent { get; init; }
        public DateTime OccurredAt { get; init; }
        public DateTime IngestedAt { get; init; }
        public EventCategory Category { get; init; }
        public UrgencyLevel Urgency { get; init; }
        public bool IsProcessed { get; set; }

        public ICollection<EventSource> Sources { get; init; } = new List<EventSource>();
        public ICollection<EventImpact> Impacts { get; init; } = new List<EventImpact>();
    }
}
