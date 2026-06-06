using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using IranRiskTracker.Application.DTOs;
using IranRiskTracker.Application.Interfaces;
using IranRiskTracker.Domain.Enums;

namespace IranRiskTracker.Application.Services
{
    /// <summary>
    /// Produces deterministic indicator-based risk snapshots from seeded JSON data.
    /// The implementation is in-memory and traceable: each indicator yields a contribution
    /// derived from the count of matching historical events, the indicator weight, and direction.
    /// </summary>
    public class RiskCalculator : IRiskCalculator
    {
        private readonly ISeedDataProvider _seedDataProvider;

        // Scoring constants for Phase 1 deterministic algorithm
        private const double ScorePerMatchingHistoricalEvent = 20.0;
        private const double MinimumRiskScore = 1.0;
        private const double MaximumRiskScore = 100.0;
        private const int ScoreRoundingDigits = 2;

        public RiskCalculator(ISeedDataProvider seedDataProvider)
        {
            _seedDataProvider = seedDataProvider;
        }

        /// <summary>
        /// Calculates a deterministic baseline risk snapshot.
        /// For each indicator, counts historical events matching the indicator's category,
        /// computes a base score (count × ScorePerMatchingHistoricalEvent), then applies
        /// the indicator weight and direction multiplier to produce a weighted contribution.
        /// All contributions are summed to a final score and returned with per-indicator trace data.
        /// </summary>
        public Task<RiskDto> GetCurrentRiskAsync()
        {
            var historicalEvents = _seedDataProvider.GetHistoricalEvents().ToList();
            var indicators = _seedDataProvider.GetIndicators().ToList();
            var contributions = new List<IndicatorRiskContributionDto>();
            double total = 0.0;

            foreach (var ind in indicators)
            {
                var matching = historicalEvents.Count(e => e.Category == ind.Category);
                var baseScore = CalculateBaseScore(matching);
                var weighted = CalculateWeightedContribution(baseScore, ind.Weight, ind.DirectionMultiplier);

                var dto = new IndicatorRiskContributionDto
                {
                    IndicatorKey = ind.Key,
                    IndicatorName = ind.Name,
                    Category = ind.Category,
                    Weight = ind.Weight,
                    MatchingHistoricalEventCount = matching,
                    BaseScore = baseScore,
                    WeightedContribution = weighted,
                    Explanation = BuildExplanation(baseScore, ind.Weight, ind.DirectionMultiplier, weighted)
                };

                contributions.Add(dto);
                total += weighted;
            }

            var final = Math.Clamp(Math.Round(total, ScoreRoundingDigits), MinimumRiskScore, MaximumRiskScore);

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

        private static double CalculateBaseScore(int matchingEventCount)
        {
            return Math.Clamp(matchingEventCount * ScorePerMatchingHistoricalEvent, 0.0, MaximumRiskScore);
        }

        private static double CalculateWeightedContribution(double baseScore, decimal indicatorWeight, int directionMultiplier)
        {
            var weighted = baseScore * (double)indicatorWeight * directionMultiplier;
            return weighted < 0 ? 0.0 : weighted;
        }

        private static string BuildExplanation(double baseScore, decimal weight, int direction, double weighted)
        {
            return $"Base={baseScore}, Weight={weight}, Direction={direction}, Weighted={weighted}";
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
