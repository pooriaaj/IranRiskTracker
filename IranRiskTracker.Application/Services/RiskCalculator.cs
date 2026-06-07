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
        private readonly IOwnerOverrideStore _overrideStore;

        // Scoring constants
        private const double ScorePerMatchingHistoricalEvent = 20.0;
        private const double LiveUrgencyLow = 5.0;
        private const double LiveUrgencyMedium = 10.0;
        private const double LiveUrgencyHigh = 20.0;
        private const double LiveUrgencyCritical = 35.0;
        private const double MinimumRiskScore = 1.0;
        private const double MaximumRiskScore = 100.0;
        private const int ScoreRoundingDigits = 2;

        public RiskCalculator(ISeedDataProvider seedDataProvider, ILiveEventStore liveStore, IOwnerOverrideStore overrideStore)
        {
            _seedDataProvider = seedDataProvider;
            _liveStore = liveStore;
            _overrideStore = overrideStore;
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
            // Use single calculation time for deterministic recency buckets
            var calculationTime = DateTime.UtcNow;
            // Load seeded sources to allow matching live event sources to their credibility.
            var sources = _seedDataProvider.GetSources().ToList();

            foreach (var ind in indicators)
            {
                var matching = historicalEvents.Count(e => e.Category == ind.Category);
                var matchingLive = liveEvents.Count(e => e.Category == ind.Category);

                var historicalBaseScore = CalculateHistoricalBaseScore(matching);
                var matchingLiveEvents = liveEvents.Where(e => e.Category == ind.Category).ToList();
                var liveBaseScore = CalculateLiveBaseScore(matchingLiveEvents, sources, calculationTime);
                // Corroboration multiplier: count distinct live source names for this indicator
                var distinctSources = matchingLiveEvents
                    .Select(e => (e.SourceName ?? string.Empty).Trim())
                    .Where(n => !string.IsNullOrEmpty(n))
                    .Select(n => n.ToLowerInvariant())
                    .Distinct()
                    .Count();
                double corroborationMultiplier = distinctSources switch
                {
                    0 => 1.0,
                    1 => 1.0,
                    2 => 1.10,
                    3 => 1.20,
                    _ => 1.30
                };
                liveBaseScore = Math.Clamp(liveBaseScore * corroborationMultiplier, 0.0, MaximumRiskScore);
                var indicatorBaseScore = Math.Clamp(historicalBaseScore + liveBaseScore, 0.0, MaximumRiskScore);
                var weighted = CalculateWeightedContribution(indicatorBaseScore, ind.Weight, ind.DirectionMultiplier);

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
                    CorroborationMultiplier = corroborationMultiplier,
                    DistinctLiveSourceCount = distinctSources,
                    BaseScore = indicatorBaseScore,
                    WeightedContribution = weighted,
                    Explanation = BuildExplanation(historicalBaseScore, liveBaseScore, ind.Weight, ind.DirectionMultiplier, weighted, matching, matchingLive)
                };

                // Build live signal DTOs using centralized helper
                var liveSignals = liveEvents.Where(e => e.Category == ind.Category)
                    .Select(e => BuildLiveSignalContribution(e, sources, calculationTime)).ToList();

                dto.LiveSignals = liveSignals;

                contributions.Add(dto);
                total += weighted;
            }

            var baseScore = Math.Clamp(Math.Round(total, ScoreRoundingDigits), MinimumRiskScore, MaximumRiskScore);

            // Load owner overrides but do not alter indicator contributions
            var overrides = _overrideStore.GetAll().ToList();
            var overrideTotal = overrides.Sum(o => o.ScoreAdjustment);

            var final = Math.Clamp(baseScore + overrideTotal, MinimumRiskScore, MaximumRiskScore);

            var result = new RiskDto
            {
                Timestamp = DateTime.UtcNow,
                Level = MapRiskLevel(final),
                Score = final,
                BaseScoreBeforeOverrides = baseScore,
                OwnerOverrideTotalAdjustment = overrideTotal,
                AppliedOwnerOverrides = overrides,
                Summary = $"Deterministic scoring using {indicators.Count} indicators, {historicalEvents.Count} historical events, {liveEvents.Count} live events and {overrides.Count} owner overrides. BaseScore={baseScore}, OverrideTotal={overrideTotal}, FinalScore={final}",
                Contributions = contributions
            };

            return Task.FromResult(result);
        }

        private static double CalculateHistoricalBaseScore(int matchingEventCount)
        {
            return Math.Clamp(matchingEventCount * ScorePerMatchingHistoricalEvent, 0.0, MaximumRiskScore);
        }

        private static double CalculateLiveBaseScore(IEnumerable<LiveEventDto> events, List<Domain.Entities.Source> sources, DateTime calculationTime)
        {
            // Reuse live signal construction to ensure identical matching and adjusted score calculation
            double sum = 0.0;
            foreach (var e in events)
            {
                var sig = BuildLiveSignalContribution(e, sources, calculationTime);
                sum += sig.RecencyAdjustedUrgencyScore;
            }

            return Math.Clamp(sum, 0.0, MaximumRiskScore);
        }

        private static Domain.Entities.Source? FindMatchingSource(LiveEventDto liveEvent, IReadOnlyCollection<Domain.Entities.Source> sources)
        {
            return sources.FirstOrDefault(s => string.Equals(s.Name?.Trim(), liveEvent.SourceName?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase));
        }

        private static double GetSourceCredibility(LiveEventDto liveEvent, IReadOnlyCollection<Domain.Entities.Source> sources)
        {
            var match = FindMatchingSource(liveEvent, sources);
            var credibility = match?.Credibility.Value ?? 0.5m;
            return (double)credibility;
        }

        private static LiveSignalContributionDto BuildLiveSignalContribution(LiveEventDto liveEvent, IReadOnlyCollection<Domain.Entities.Source> sources, DateTime? calculationTime = null)
        {
            var match = FindMatchingSource(liveEvent, sources);
            var credibility = match?.Credibility.Value ?? 0.5m;
            var urgencyScore = GetUrgencyScore(liveEvent.Urgency);
            var adjusted = (double)credibility * urgencyScore;

            // Determine recency multiplier using deterministic buckets based on calculationTime
            var now = calculationTime ?? DateTime.UtcNow;
            var age = now - liveEvent.OccurredAt;
            double recencyMultiplier;
            var ageHours = age.TotalHours;
            if (ageHours <= 6.0) recencyMultiplier = 1.0;
            else if (ageHours <= 24.0) recencyMultiplier = 0.75;
            else if (ageHours <= 72.0) recencyMultiplier = 0.50;
            else recencyMultiplier = 0.25;

            var recencyAdjusted = adjusted * recencyMultiplier;

            return new LiveSignalContributionDto
            {
                LiveEventId = liveEvent.Id,
                Title = liveEvent.Title,
                Category = liveEvent.Category,
                Urgency = liveEvent.Urgency,
                UrgencyScore = urgencyScore,
                SourceCredibility = (double)credibility,
                CredibilityAdjustedUrgencyScore = adjusted,
                RecencyMultiplier = recencyMultiplier,
                RecencyAdjustedUrgencyScore = recencyAdjusted,
                SourceMatchedFromSeed = match != null,
                SourceName = liveEvent.SourceName,
                SourceUrl = liveEvent.SourceUrl,
                SourceHandle = liveEvent.SourceHandle,
                OwnerNotes = liveEvent.OwnerNotes,
                OccurredAt = liveEvent.OccurredAt,
                IngestedAt = liveEvent.IngestedAt
            };
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
            return $"HistCount={historicalCount}, LiveCount={liveCount}, HistBase={historicalBase}, LiveBase={liveBase} (credibility+recency-adjusted), Weight={weight}, Direction={direction}, Weighted={weighted}";
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
