using System;

namespace IranRiskTracker.Domain.Entities
{
    /// <summary>
    /// Joins an event (historical or live) to its sources with a corroboration weight.
    /// </summary>
    public class EventSource
    {
        public Guid EventId { get; init; }
        public Guid SourceId { get; init; }
        public decimal CorroborationWeight { get; init; } = 1m;

        public Source? Source { get; init; }
    }
}
