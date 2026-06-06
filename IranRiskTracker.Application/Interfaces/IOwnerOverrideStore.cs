using System.Collections.Generic;
using IranRiskTracker.Application.DTOs;

namespace IranRiskTracker.Application.Interfaces
{
    public interface IOwnerOverrideStore
    {
        IReadOnlyCollection<OwnerOverrideDto> GetAll();
        OwnerOverrideDto Add(OwnerOverrideDto ownerOverride);
    }
}
