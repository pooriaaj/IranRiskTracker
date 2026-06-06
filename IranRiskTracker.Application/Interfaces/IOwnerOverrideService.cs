using System.Collections.Generic;
using IranRiskTracker.Application.DTOs;

namespace IranRiskTracker.Application.Interfaces
{
    public interface IOwnerOverrideService
    {
        IReadOnlyCollection<OwnerOverrideDto> GetAll();
        OwnerOverrideDto Add(OwnerOverrideCreateRequest request);
    }
}
