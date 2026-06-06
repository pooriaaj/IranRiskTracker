using System.Collections.Generic;
using IranRiskTracker.Application.DTOs;

namespace IranRiskTracker.Application.Interfaces
{
    /// <summary>
    /// Coordinates event reads and Phase 1 live-event response shaping for API controllers.
    /// </summary>
    public interface IEventQueryService
    {
        IEnumerable<HistoricalEventDto> GetHistoricalEvents();
        IEnumerable<LiveEventDto> GetLiveEvents();
        LiveEventDto AcceptLiveEvent(LiveEventCreateRequest request);
    }
}
