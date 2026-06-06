using System.Collections.Generic;
using System.Linq;
using IranRiskTracker.Application.Interfaces;
using IranRiskTracker.Application.DTOs;

namespace IranRiskTracker.Infrastructure.Storage
{
    /// <summary>
    /// Simple thread-safe in-memory store for LiveEventDto objects.
    /// Data lives for the application lifetime; no persistence is performed.
    /// </summary>
    public class InMemoryLiveEventStore : ILiveEventStore
    {
        private readonly List<LiveEventDto> _items = new();
        private readonly object _lock = new();

        public LiveEventDto Add(LiveEventDto liveEvent)
        {
            lock (_lock)
            {
                _items.Add(liveEvent);
                return liveEvent;
            }
        }

        public IReadOnlyCollection<LiveEventDto> GetAll()
        {
            lock (_lock)
            {
                // newest first
                return _items.OrderByDescending(i => i.IngestedAt).ToList().AsReadOnly();
            }
        }
    }
}
