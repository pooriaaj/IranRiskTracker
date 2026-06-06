using System.Collections.Generic;
using IranRiskTracker.Application.DTOs;

namespace IranRiskTracker.Application.Interfaces
{
    /// <summary>
    /// Abstraction for querying events in a JSON-first world. Keeps controllers free of file IO.
    /// </summary>
    public interface IEventQueryService
    {
        IEnumerable<HistoricalEventDto> GetHistoricalEvents();
        IEnumerable<LiveEventDto> GetLiveEvents();
    }
}
