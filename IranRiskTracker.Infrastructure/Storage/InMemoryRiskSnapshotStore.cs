using System.Collections.Generic;
using System.Linq;
using IranRiskTracker.Application.DTOs;
using IranRiskTracker.Application.Interfaces;

namespace IranRiskTracker.Infrastructure.Storage
{
    public class InMemoryRiskSnapshotStore : IRiskSnapshotStore
    {
        private readonly List<RiskDto> _items = new();
        private readonly object _lock = new();

        public RiskDto Add(RiskDto snapshot)
        {
            lock (_lock)
            {
                _items.Add(snapshot);
                return snapshot;
            }
        }

        public RiskDto? GetLatest()
        {
            lock (_lock)
            {
                return _items.OrderByDescending(i => i.Timestamp).FirstOrDefault();
            }
        }

        public IReadOnlyCollection<RiskDto> GetAll()
        {
            lock (_lock)
            {
                return _items.OrderByDescending(i => i.Timestamp).ToList().AsReadOnly();
            }
        }
    }
}
