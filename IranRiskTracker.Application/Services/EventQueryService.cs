using System.Collections.Generic;
using System.Linq;
using IranRiskTracker.Application.Interfaces;
using IranRiskTracker.Application.DTOs;
using IranRiskTracker.Domain.Entities;

namespace IranRiskTracker.Application.Services
{
    /// <summary>
    /// Application-level service that maps seeded domain entities into DTOs
    /// for consumption by API controllers.
    /// </summary>
    public class EventQueryService : IEventQueryService
    {
        private readonly ISeedDataProvider _seed;

        public EventQueryService(ISeedDataProvider seed)
        {
            _seed = seed;
        }

        public IEnumerable<HistoricalEventDto> GetHistoricalEvents()
        {
            var items = _seed.GetHistoricalEvents();

            return items.Select(e => new HistoricalEventDto
            {
                Id = e.Id,
                OccurredAt = e.OccurredAt,
                Title = e.Title,
                Details = e.Description,
                Severity = IranRiskTracker.Domain.Enums.RiskLevel.Unknown,
                Category = e.Category,
                RegionTag = e.RegionTag,
                VerifiedAt = e.VerifiedAt,
                IsBaseline = e.IsBaseline,
                SourceCount = e.Sources?.Count ?? 0,
                ImpactCount = e.Impacts?.Count ?? 0
            });
        }

        public IEnumerable<LiveEventDto> GetLiveEvents()
        {
            // Phase 1: No live events persisted; return empty.
            return Enumerable.Empty<LiveEventDto>();
        }
    }
}
