using System;
using IranRiskTracker.Domain.Enums;

namespace IranRiskTracker.Application.DTOs
{
    /// <summary>
    /// Public API projection for a verified historical baseline event.
    /// </summary>
    public class HistoricalEventDto
    {
        public Guid Id { get; set; }
        public DateTime OccurredAt { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public EventCategory Category { get; set; }
        public string? RegionTag { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public bool IsBaseline { get; set; }
        public int SourceCount { get; set; }
        public int ImpactCount { get; set; }
    }
}
