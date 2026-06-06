using System;
using IranRiskTracker.Domain.Enums;

namespace IranRiskTracker.Application.DTOs
{
    /// <summary>
    /// Public API projection for a live event accepted by the Phase 1 API.
    /// </summary>
    public class LiveEventDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? RawContent { get; set; }
        public DateTime OccurredAt { get; set; }
        public DateTime IngestedAt { get; set; }
        public EventCategory Category { get; set; }
        public UrgencyLevel Urgency { get; set; }
        public bool IsProcessed { get; set; }
    }
}
