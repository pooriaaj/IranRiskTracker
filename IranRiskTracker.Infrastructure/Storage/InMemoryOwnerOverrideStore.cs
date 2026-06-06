using System.Collections.Generic;
using System.Linq;
using IranRiskTracker.Application.DTOs;
using IranRiskTracker.Application.Interfaces;

namespace IranRiskTracker.Infrastructure.Storage
{
    public class InMemoryOwnerOverrideStore : IOwnerOverrideStore
    {
        private readonly List<OwnerOverrideDto> _items = new();
        private readonly object _lock = new();

        public OwnerOverrideDto Add(OwnerOverrideDto ownerOverride)
        {
            lock (_lock)
            {
                _items.Add(ownerOverride);
                return ownerOverride;
            }
        }

        public IReadOnlyCollection<OwnerOverrideDto> GetAll()
        {
            lock (_lock)
            {
                return _items.OrderByDescending(i => i.AppliedAt).ToList().AsReadOnly();
            }
        }
    }
}
