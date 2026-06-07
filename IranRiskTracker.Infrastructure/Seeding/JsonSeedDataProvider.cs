using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Linq;
using IranRiskTracker.Application.Interfaces;
using IranRiskTracker.Domain.Entities;

namespace IranRiskTracker.Infrastructure.Seeding
{
    /// <summary>
    /// Reads JSON seed files from disk and exposes them through the application seed contract.
    /// </summary>
    public class JsonSeedDataProvider : ISeedDataProvider
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        static JsonSeedDataProvider()
        {
            SerializerOptions.Converters.Add(new CredibilityScoreJsonConverter());
            // Allow enum values to be read from their string names in seed JSON files
            SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        private readonly string _basePath;

        public JsonSeedDataProvider(string basePath)
        {
            _basePath = basePath;
        }

        /// <summary>
        /// Loads historical baseline events from historical_events.json.
        /// </summary>
        public IEnumerable<HistoricalEvent> GetHistoricalEvents()
        {
            return LoadSeedFile<HistoricalEvent>("historical_events.json");
        }

        /// <summary>
        /// Loads source metadata from sources.json.
        /// </summary>
        public IEnumerable<Source> GetSources()
        {
            return LoadSeedFile<Source>("sources.json");
        }

        /// <summary>
        /// Loads indicator definitions from indicators.json.
        /// </summary>
        public IEnumerable<Indicator> GetIndicators()
        {
            return LoadSeedFile<Indicator>("indicators.json");
        }

        /// <summary>
        /// Loads event impact definitions from event_impacts.json.
        /// </summary>
        public IEnumerable<EventImpact> GetEventImpacts()
        {
            return LoadSeedFile<EventImpact>("event_impacts.json");
        }

        private IEnumerable<T> LoadSeedFile<T>(string fileName)
        {
            var path = Path.Combine(_basePath, fileName);
            if (!File.Exists(path))
            {
                return Enumerable.Empty<T>();
            }

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<T>>(json, SerializerOptions) ?? Enumerable.Empty<T>();
        }
    }
}
