using System.Collections.Generic;
using IranRiskTracker.Domain.Entities;

namespace IranRiskTracker.Application.Interfaces
{
    /// <summary>
    /// Provides access to seeded domain data.
    /// </summary>
    public interface ISeedDataProvider
    {
        IEnumerable<HistoricalEvent> GetHistoricalEvents();
        IEnumerable<Domain.Entities.Source> GetSources();
        IEnumerable<Domain.Entities.Indicator> GetIndicators();
    }
}
