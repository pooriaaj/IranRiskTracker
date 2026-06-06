using System;
using IranRiskTracker.Domain.Enums;

namespace IranRiskTracker.Application.DTOs
{
    /// <summary>
    /// Lightweight DTO describing a live event for API consumers.
    /// </summary>
    public class LiveEventDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? RawContent { get; set; }
        public DateTime OccurredAt { get; set; }
        public UrgencyLevel Urgency { get; set; }
        public EventCategory Category { get; set; }
        public bool IsProcessed { get; set; }
    }
}
