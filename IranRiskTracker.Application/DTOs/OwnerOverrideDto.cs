using System;
using IranRiskTracker.Domain.Enums;

namespace IranRiskTracker.Application.DTOs
{
    public class OwnerOverrideDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;
        public EventCategory Category { get; set; }
        public double ScoreAdjustment { get; set; }
        public DateTime AppliedAt { get; set; }
        public string? SourceReference { get; set; }
    }
}
