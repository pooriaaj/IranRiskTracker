using System.Collections.Generic;
using IranRiskTracker.Application.DTOs;

namespace IranRiskTracker.Application.Interfaces
{
    public interface IRiskSnapshotStore
    {
        RiskDto Add(RiskDto snapshot);
        RiskDto? GetLatest();
        IReadOnlyCollection<RiskDto> GetAll();
    }
}
