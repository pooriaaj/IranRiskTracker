using System;
using IranRiskTracker.Domain.Enums;

namespace IranRiskTracker.Application.DTOs
{
    /// <summary>
    /// Small DTO representing the minimal payload for ingesting a live event.
    /// Phase 1: simple mutable POCO for controller model binding.
    /// </summary>
    public class LiveEventCreateRequest
    {
        public string? Title { get; set; }
        public string? RawContent { get; set; }
        public DateTime OccurredAt { get; set; }
        public UrgencyLevel Urgency { get; set; }
        public EventCategory Category { get; set; }
    }
}
