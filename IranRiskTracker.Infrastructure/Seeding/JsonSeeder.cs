using System.Text.Json;
using IranRiskTracker.Domain.Entities;

namespace IranRiskTracker.Infrastructure.Seeding
{
    /// <summary>
    /// Reads JSON files from the Data folder and produces domain objects.
    /// Phase 1: simple synchronous loader; validating and mapping to domain
    /// objects will be extended in Phase 2.
    /// </summary>
    public static class JsonSeeder
    {
        private static readonly JsonSerializerOptions _opts = new() { PropertyNameCaseInsensitive = true };

        public static IEnumerable<HistoricalEvent> LoadHistoricalEvents(string basePath)
        {
            var path = Path.Combine(basePath, "historical_events.json");
            if (!File.Exists(path)) return Enumerable.Empty<HistoricalEvent>();

            var json = File.ReadAllText(path);
            var items = JsonSerializer.Deserialize<List<HistoricalEvent>>(json, _opts);
            return items ?? Enumerable.Empty<HistoricalEvent>();
        }

        public static IEnumerable<Source> LoadSources(string basePath)
        {
            var path = Path.Combine(basePath, "sources.json");
            if (!File.Exists(path)) return Enumerable.Empty<Source>();

            var json = File.ReadAllText(path);
            var items = JsonSerializer.Deserialize<List<Source>>(json, _opts);
            return items ?? Enumerable.Empty<Source>();
        }

        public static IEnumerable<Indicator> LoadIndicators(string basePath)
        {
            var path = Path.Combine(basePath, "indicators.json");
            if (!File.Exists(path)) return Enumerable.Empty<Indicator>();

            var json = File.ReadAllText(path);
            var items = JsonSerializer.Deserialize<List<Indicator>>(json, _opts);
            return items ?? Enumerable.Empty<Indicator>();
        }
    }
}
