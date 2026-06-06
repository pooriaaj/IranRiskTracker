using IranRiskTracker.Domain.ValueObjects;
using IranRiskTracker.Domain.Enums;
using System;
using System.Collections.Generic;

namespace IranRiskTracker.Domain.Entities
{
    /// <summary>
    /// A named information origin with a credibility weight the scoring engine uses
    /// to adjust raw event delta.
    /// </summary>
    public class Source
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Url { get; init; }
        public CredibilityScore Credibility { get; init; }
        public SourceBias Bias { get; init; }
        public bool IsActive { get; init; } = true;

        public ICollection<EventSource> EventLinks { get; init; } = new List<EventSource>();
    }
}
