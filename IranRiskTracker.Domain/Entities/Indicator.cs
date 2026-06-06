using System;
using IranRiskTracker.Domain.ValueObjects;

namespace IranRiskTracker.Domain.Entities
{
    /// <summary>
    /// Represents an indicator which can influence risk scoring.
    /// This is a parent entity for indicator configuration.
    /// </summary>
    public class Indicator
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = string.Empty; // unique key
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Source Source { get; set; }

        public Indicator(Guid id, string key, string name, Source source)
        {
            Id = id;
            Key = key;
            Name = name;
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }
    }
}
