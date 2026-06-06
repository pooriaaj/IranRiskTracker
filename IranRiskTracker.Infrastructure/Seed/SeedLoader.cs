using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using IranRiskTracker.Domain.Entities;

namespace IranRiskTracker.Infrastructure.Seed
{
    /// <summary>
    /// Skeleton JSON seed loader. Responsible for loading seed files from disk
    /// and producing domain entities. For Phase 1 this is JSON-first and synchronous.
    /// </summary>
    public static class SeedLoader
    {
        public static IEnumerable<HistoricalEvent> LoadHistoricalEvents(string jsonPath)
        {
            if (!File.Exists(jsonPath)) return Enumerable.Empty<HistoricalEvent>();

            var json = File.ReadAllText(jsonPath);
            var items = JsonSerializer.Deserialize<List<HistoricalEvent>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return items ?? Enumerable.Empty<HistoricalEvent>();
        }
    }
}
