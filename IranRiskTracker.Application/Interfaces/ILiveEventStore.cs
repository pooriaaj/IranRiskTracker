using System.Collections.Generic;
using IranRiskTracker.Application.DTOs;

namespace IranRiskTracker.Application.Interfaces
{
    /// <summary>
    /// Abstraction for transient in-memory storage of live events during app lifetime.
    /// </summary>
    public interface ILiveEventStore
    {
        IReadOnlyCollection<LiveEventDto> GetAll();
        LiveEventDto Add(LiveEventDto liveEvent);
    }
}
