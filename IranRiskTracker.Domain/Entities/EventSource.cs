using System;

namespace IranRiskTracker.Domain.Entities
{
    /// <summary>
    /// Joins an event (historical or live) to its sources with a corroboration weight.
    /// </summary>
    public class EventSource
    {
        public Guid EventId { get; init; }
        /// <summary>
        /// Required alongside EventId to resolve which event table this record targets.
        /// </summary>
        public IranRiskTracker.Domain.Enums.EventType EventType { get; init; }
        public Guid SourceId { get; init; }
        public decimal CorroborationWeight { get; init; } = 1m;

        public Source? Source { get; init; }
    }
}
