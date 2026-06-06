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

            var contributions = new System.Collections.Generic.List<IndicatorRiskContributionDto>();

            double total = 0.0;

            foreach (var ind in indicators)
            {
                var matching = historicalEvents.Count(e => e.Category == ind.Category);
                var baseScore = Math.Clamp(matching * 20.0, 0.0, 100.0);
                var weighted = baseScore * (double)ind.Weight * ind.DirectionMultiplier;
                if (weighted < 0) weighted = 0; // clamp negatives for Phase 1

                var dto = new IndicatorRiskContributionDto
                {
                    IndicatorKey = ind.Key,
                    IndicatorName = ind.Name,
                    Category = ind.Category,
                    Weight = ind.Weight,
                    MatchingHistoricalEventCount = matching,
                    BaseScore = baseScore,
                    WeightedContribution = weighted,
                    Explanation = $"Base={baseScore}, Weight={ind.Weight}, Direction={ind.DirectionMultiplier}, Weighted={weighted}"
                };

                contributions.Add(dto);
                total += weighted;
            }

            var final = Math.Clamp(Math.Round(total, 2), 1.0, 100.0);

            var result = new RiskDto
            {
                Timestamp = DateTime.UtcNow,
                Level = MapRiskLevel(final),
                Score = final,
                Summary = $"Deterministic seed-based scoring using {indicators.Count} indicators and {historicalEvents.Count} historical events.",
                Contributions = contributions
            };

            return Task.FromResult(result);
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
// End of file
