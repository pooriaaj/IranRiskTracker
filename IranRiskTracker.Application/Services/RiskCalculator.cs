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
        private readonly ILiveEventStore _liveStore;

        // Scoring constants
        private const double ScorePerMatchingHistoricalEvent = 20.0;
        private const double LiveUrgencyLow = 5.0;
        private const double LiveUrgencyMedium = 10.0;
        private const double LiveUrgencyHigh = 20.0;
        private const double LiveUrgencyCritical = 35.0;
        private const double MinimumRiskScore = 1.0;
        private const double MaximumRiskScore = 100.0;
        private const int ScoreRoundingDigits = 2;

        public RiskCalculator(ISeedDataProvider seedDataProvider, ILiveEventStore liveStore)
        {
            _seedDataProvider = seedDataProvider;
            _liveStore = liveStore;
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

            var liveEvents = _liveStore.GetAll().ToList();

            foreach (var ind in indicators)
            {
                var matching = historicalEvents.Count(e => e.Category == ind.Category);
                var matchingLive = liveEvents.Count(e => e.Category == ind.Category);

                var historicalBaseScore = CalculateHistoricalBaseScore(matching);
                var liveBaseScore = CalculateLiveBaseScore(liveEvents.Where(e => e.Category == ind.Category));
                var baseScore = Math.Clamp(historicalBaseScore + liveBaseScore, 0.0, MaximumRiskScore);
                var weighted = CalculateWeightedContribution(baseScore, ind.Weight, ind.DirectionMultiplier);

                var dto = new IndicatorRiskContributionDto
                {
                    IndicatorKey = ind.Key,
                    IndicatorName = ind.Name,
                    Category = ind.Category,
                    Weight = ind.Weight,
                    MatchingHistoricalEventCount = matching,
                    MatchingLiveEventCount = matchingLive,
                    HistoricalBaseScore = historicalBaseScore,
                    LiveBaseScore = liveBaseScore,
                    BaseScore = baseScore,
                    WeightedContribution = weighted,
                    Explanation = BuildExplanation(historicalBaseScore, liveBaseScore, ind.Weight, ind.DirectionMultiplier, weighted, matching, matchingLive)
                };

                // Build live signal DTOs
                var liveSignals = liveEvents.Where(e => e.Category == ind.Category)
                    .Select(e => new LiveSignalContributionDto
                    {
                        LiveEventId = e.Id,
                        Title = e.Title,
                        Category = e.Category,
                        Urgency = e.Urgency,
                        UrgencyScore = GetUrgencyScore(e.Urgency),
                        SourceName = e.SourceName,
                        SourceUrl = e.SourceUrl,
                        SourceHandle = e.SourceHandle,
                        OwnerNotes = e.OwnerNotes,
                        OccurredAt = e.OccurredAt,
                        IngestedAt = e.IngestedAt
                    }).ToList();

                dto.LiveSignals = liveSignals;

                contributions.Add(dto);
                total += weighted;
            }

            var final = Math.Clamp(Math.Round(total, ScoreRoundingDigits), MinimumRiskScore, MaximumRiskScore);

            var result = new RiskDto
            {
                Timestamp = DateTime.UtcNow,
                Level = MapRiskLevel(final),
                Score = final,
                Summary = $"Deterministic scoring using {indicators.Count} indicators, {historicalEvents.Count} historical events and {liveEvents.Count} live events.",
                Contributions = contributions
            };

            return Task.FromResult(result);
        }

        private static double CalculateHistoricalBaseScore(int matchingEventCount)
        {
            return Math.Clamp(matchingEventCount * ScorePerMatchingHistoricalEvent, 0.0, MaximumRiskScore);
        }

        private static double CalculateLiveBaseScore(IEnumerable<LiveEventDto> events)
        {
            double sum = 0.0;
            foreach (var e in events)
            {
                sum += GetUrgencyScore(e.Urgency);
            }

            return Math.Clamp(sum, 0.0, MaximumRiskScore);
        }

        private static double GetUrgencyScore(UrgencyLevel urgency)
        {
            return urgency switch
            {
                UrgencyLevel.Low => LiveUrgencyLow,
                UrgencyLevel.Medium => LiveUrgencyMedium,
                UrgencyLevel.High => LiveUrgencyHigh,
                UrgencyLevel.Critical => LiveUrgencyCritical,
                _ => 0.0
            };
        }

        private static double CalculateWeightedContribution(double baseScore, decimal indicatorWeight, int directionMultiplier)
        {
            var weighted = baseScore * (double)indicatorWeight * directionMultiplier;
            return weighted < 0 ? 0.0 : weighted;
        }

        private static string BuildExplanation(double historicalBase, double liveBase, decimal weight, int direction, double weighted, int historicalCount, int liveCount)
        {
            return $"HistCount={historicalCount}, LiveCount={liveCount}, HistBase={historicalBase}, LiveBase={liveBase}, Weight={weight}, Direction={direction}, Weighted={weighted}";
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
