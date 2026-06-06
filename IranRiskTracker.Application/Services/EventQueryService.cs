using System.Collections.Generic;
using System.Linq;
using IranRiskTracker.Application.DTOs;
using IranRiskTracker.Application.Interfaces;

namespace IranRiskTracker.Application.Services
{
    /// <summary>
    /// Maps event seed data into API DTOs and keeps controllers independent from storage details.
    /// </summary>
    public class EventQueryService : IEventQueryService
    {
        private readonly ISeedDataProvider _seedDataProvider;

        public EventQueryService(ISeedDataProvider seedDataProvider)
        {
            _seedDataProvider = seedDataProvider;
        }

        /// <summary>
        /// Returns historical seed events with only API-safe summary metadata.
        /// </summary>
        public IEnumerable<HistoricalEventDto> GetHistoricalEvents()
        {
            return _seedDataProvider.GetHistoricalEvents().Select(e => new HistoricalEventDto
            {
                Id = e.Id,
                OccurredAt = e.OccurredAt,
                Title = e.Title,
                Description = e.Description, 
                Category = e.Category,
                RegionTag = e.RegionTag,
                VerifiedAt = e.VerifiedAt,
                IsBaseline = e.IsBaseline,
                SourceCount = e.Sources?.Count ?? 0,
                ImpactCount = e.Impacts?.Count ?? 0
            });
        }

        /// <summary>
        /// Returns live events when ingestion is introduced; Phase 1 has no live store.
        /// </summary>
        public IEnumerable<LiveEventDto> GetLiveEvents()
        {
            return Enumerable.Empty<LiveEventDto>();
        }

        /// <summary>
        /// Creates the transient live-event response used before persistence exists.
        /// </summary>
        public LiveEventDto AcceptLiveEvent(LiveEventCreateRequest request)
        {
            return new LiveEventDto
            {
                Id = Guid.NewGuid(),
                Title = request.Title?.Trim() ?? string.Empty,
                RawContent = request.RawContent,
                OccurredAt = request.OccurredAt,
                IngestedAt = DateTime.UtcNow,
                Category = request.Category,
                Urgency = request.Urgency,
                IsProcessed = false
            };
        }
    }
}
