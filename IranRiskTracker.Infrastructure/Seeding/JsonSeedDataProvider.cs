using System.Collections.Generic;
using IranRiskTracker.Application.Interfaces;
using IranRiskTracker.Domain.Entities;

namespace IranRiskTracker.Infrastructure.Seeding
{
    /// <summary>
    /// JSON-first seed data provider reading from the Seeding/Data folder.
    /// </summary>
    public class JsonSeedDataProvider : ISeedDataProvider
    {
        private readonly string _basePath;

        public JsonSeedDataProvider(string basePath)
        {
            _basePath = basePath;
        }

        public IEnumerable<HistoricalEvent> GetHistoricalEvents()
            => JsonSeeder.LoadHistoricalEvents(_basePath);

        public IEnumerable<Source> GetSources()
            => JsonSeeder.LoadSources(_basePath);

        public IEnumerable<Indicator> GetIndicators()
            => JsonSeeder.LoadIndicators(_basePath);
    }
}
