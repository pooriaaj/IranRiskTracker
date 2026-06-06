using System;
using IranRiskTracker.Domain.Enums;

namespace IranRiskTracker.Application.DTOs
{
    /// <summary>
    /// DTO for exposing historical events via the API.
    /// </summary>
    public class HistoricalEventDto
    {
        public Guid Id { get; set; }
        public DateTime OccurredAt { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Details { get; set; }
        public RiskLevel Severity { get; set; }
        public EventCategory Category { get; set; }
        public string? RegionTag { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public bool IsBaseline { get; set; }
        public int SourceCount { get; set; }
        public int ImpactCount { get; set; }
    }
}
