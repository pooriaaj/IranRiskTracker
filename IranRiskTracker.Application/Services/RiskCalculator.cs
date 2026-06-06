using System.Threading.Tasks;
using IranRiskTracker.Application.DTOs;
using IranRiskTracker.Application.Interfaces;
using IranRiskTracker.Domain.Enums;

namespace IranRiskTracker.Application.Services
{
    /// <summary>
    /// Produces a seed-backed baseline risk snapshot until the full scoring engine exists.
    /// </summary>
    public class RiskCalculator : IRiskCalculator
    {
        private readonly ISeedDataProvider _seedDataProvider;

        public RiskCalculator(ISeedDataProvider seedDataProvider)
        {
            _seedDataProvider = seedDataProvider;
        }

        /// <summary>
        /// Calculates a non-zero placeholder score from historical seed density and indicator coverage.
        /// </summary>
        public Task<RiskDto> GetCurrentRiskAsync()
        {
            var historicalEvents = _seedDataProvider.GetHistoricalEvents().ToList();
            var indicators = _seedDataProvider.GetIndicators().ToList();

            var baselineEventCount = historicalEvents.Count(e => e.IsBaseline);
            var categoryBreadth = historicalEvents.Select(e => e.Category).Distinct().Count();
            var indicatorWeight = indicators.Sum(i => Math.Max(0m, i.Weight));
            var directionalCoverage = indicators.Count(i => i.DirectionMultiplier != 0);
            var coverageRatio = indicators.Count == 0 ? 0.0 : (double)directionalCoverage / indicators.Count;
            var rawScore =
                baselineEventCount * 6.0 +
                categoryBreadth * 3.0 +
                (double)indicatorWeight * 18.0 +
                coverageRatio * 8.0;
            var score = Math.Clamp(Math.Round(rawScore, 1), 1.0, 100.0);

            var dto = new RiskDto
            {
                Timestamp = DateTime.UtcNow,
                Level = MapRiskLevel(score),
                Score = score,
                Summary = $"Seed baseline from {baselineEventCount} historical events and {indicators.Count} indicators."
            };

            return Task.FromResult(dto);
        }

        private static RiskLevel MapRiskLevel(double score)
        {
            return score switch
            {
                < 25.0 => RiskLevel.Low,
                < 50.0 => RiskLevel.Medium,
                < 75.0 => RiskLevel.High,
                _ => RiskLevel.Critical
            };
        }
    }
}
