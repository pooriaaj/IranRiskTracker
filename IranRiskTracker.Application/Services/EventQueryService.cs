using System.Collections.Generic;
using System.Linq;
using System;
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
        private readonly ILiveEventStore _liveStore;

        public EventQueryService(ISeedDataProvider seedDataProvider, ILiveEventStore liveStore)
        {
            _seedDataProvider = seedDataProvider;
            _liveStore = liveStore;
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
            return _liveStore.GetAll();
        }

        /// <summary>
        /// Creates the transient live-event response used before persistence exists.
        /// </summary>
        public LiveEventDto AcceptLiveEvent(LiveEventCreateRequest request)
        {
            var errors = IranRiskTracker.Application.Validation.LiveEventRequestValidator.Validate(request);
            if (errors != null && errors.Count > 0)
            {
                throw new ArgumentException(string.Join("; ", errors));
            }

            var live = new LiveEventDto
            {
                Id = Guid.NewGuid(),
                Title = request.Title!.Trim(),
                RawContent = request.RawContent ?? string.Empty,
                SourceName = request.SourceName?.Trim() ?? string.Empty,
                SourceUrl = request.SourceUrl,
                SourceHandle = request.SourceHandle?.Trim(),
                OwnerNotes = request.OwnerNotes,
                OccurredAt = request.OccurredAt,
                IngestedAt = DateTime.UtcNow,
                Category = request.Category,
                Urgency = request.Urgency,
                IsProcessed = false
            };

            return _liveStore.Add(live);
        }
    }
}
