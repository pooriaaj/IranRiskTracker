using System;
using IranRiskTracker.Domain.Enums;

namespace IranRiskTracker.Application.DTOs
{
    public class LiveSignalContributionDto
    {
        public Guid LiveEventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public EventCategory Category { get; set; }
        public UrgencyLevel Urgency { get; set; }
        public double UrgencyScore { get; set; }
        public double SourceCredibility { get; set; }
        public double CredibilityAdjustedUrgencyScore { get; set; }
        public double RecencyMultiplier { get; set; }
        public double RecencyAdjustedUrgencyScore { get; set; }
        public bool SourceMatchedFromSeed { get; set; }
        public string SourceName { get; set; } = string.Empty;
        public string? SourceUrl { get; set; }
        public string? SourceHandle { get; set; }
        public string? OwnerNotes { get; set; }
        public DateTime OccurredAt { get; set; }
        public DateTime IngestedAt { get; set; }
    }
}
