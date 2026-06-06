using System;
using IranRiskTracker.Domain.Enums;

namespace IranRiskTracker.Application.DTOs
{
    /// <summary>
    /// Minimal request body for accepting an incoming live event.
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
