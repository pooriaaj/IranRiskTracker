using System.Collections.Generic;
using IranRiskTracker.Domain.Entities;

namespace IranRiskTracker.Application.Interfaces
{
    /// <summary>
    /// Provides JSON-first seed data to application services without exposing file access.
    /// </summary>
    public interface ISeedDataProvider
    {
        IEnumerable<HistoricalEvent> GetHistoricalEvents();
        IEnumerable<Source> GetSources();
        IEnumerable<Indicator> GetIndicators();
    }
}
