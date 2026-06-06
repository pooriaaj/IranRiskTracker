using System;

namespace IranRiskTracker.Domain.ValueObjects
{
    /// <summary>
    /// Lightweight value object representing an information source.
    /// Immutable.
    /// </summary>
    public sealed class Source
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public string? Description { get; init; }

        public Source(Guid id, string name, string? description = null)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
        }
    }
}
