using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using IranRiskTracker.Infrastructure.Seeding;
using IranRiskTracker.Application.Services;
using IranRiskTracker.Application.DTOs;

namespace IranRiskTracker.Tests.Phase1
{
    /// <summary>
    /// Regression tests for deterministic scoring engine v1.
    /// Validates contribution generation and expected seed-based totals.
    /// </summary>
    public class RiskCalculatorV1Tests
    {
        private static string FindSeedDataPath()
        {
            var dir = new System.IO.DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
            while (dir != null)
            {
                var candidate = System.IO.Path.Combine(dir.FullName, "IranRiskTracker.Infrastructure", "Seeding", "Data");
                if (System.IO.Directory.Exists(candidate)) return candidate;
                dir = dir.Parent;
            }

            throw new System.IO.DirectoryNotFoundException("Seeding/Data folder not found");
        }

        [Fact]
        public void RiskCalculator_ShouldReturnContributionForEveryIndicator()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new IranRiskTracker.Infrastructure.Storage.InMemoryLiveEventStore();
            var overrideStore = new IranRiskTracker.Infrastructure.Storage.InMemoryOwnerOverrideStore();
            var snapshotStore = new IranRiskTracker.Infrastructure.Storage.InMemoryRiskSnapshotStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore, snapshotStore);
            // Patch: added no-op comment to ensure liveStore is passed (keeps behavior unchanged)
            // No-op: keep line numbers stable for test expectations
            // Patch: no-op insertion to ensure patch applies cleanly
            // Patch: additional no-op comment to force a deterministic edit

            var result = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();

            result.Contributions.Should().NotBeNull();
            var indicators = seed.GetIndicators().ToList();
            result.Contributions.Count.Should().Be(indicators.Count);

            foreach (var c in result.Contributions)
            {
                c.IndicatorKey.Should().NotBeNullOrWhiteSpace();
                c.IndicatorName.Should().NotBeNullOrWhiteSpace();
                c.Explanation.Should().NotBeNullOrWhiteSpace();
                c.BaseScore.Should().BeGreaterOrEqualTo(0.0).And.BeLessOrEqualTo(100.0);
                c.WeightedContribution.Should().BeGreaterOrEqualTo(0.0);
            }
        }

        [Fact]
        public void RiskCalculator_ShouldProduceExpectedSeedScore()
        {
            // Arrange: 72 Iran historical events (2000-2026 incl. Jan massacre, IRGC coup, Pahlavi plan, US strikes), 76 event impacts calibrated to ~98.45% baseline
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new IranRiskTracker.Infrastructure.Storage.InMemoryLiveEventStore();
            var overrideStore = new IranRiskTracker.Infrastructure.Storage.InMemoryOwnerOverrideStore();
            var snapshotStore = new IranRiskTracker.Infrastructure.Storage.InMemoryRiskSnapshotStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore, snapshotStore);

            // Act
            var result = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();

            // Find contributions
            var cyber = result.Contributions.Single(c => c.IndicatorKey == "cyber_incidents");
            var military = result.Contributions.Single(c => c.IndicatorKey == "military_activity");

            // Expected base scores from event_impacts.json sums (no live events)
            // cyber: 15+9+14+12+14+12+7+7 = 90 (no clamp: 90*1.05=94.5 < 100)
            // military: 18+16+10+10+10+11+15+13 = 103 -> clamped to 100
            cyber.BaseScore.Should().Be(90.0);
            military.BaseScore.Should().Be(100.0);

            var cyberSeverity = 1.05;
            var militarySeverity = 1.30;

            // Cyber: no clamping (94.5 < 100)
            cyber.WeightedContribution.Should().BeApproximately(cyber.BaseScore * cyberSeverity * (double)cyber.Weight, 0.0001);
            // Military: severity-adjusted = min(100*1.30, 100) = 100 -> weighted = 100*0.15 = 15.0
            military.WeightedContribution.Should().BeApproximately(Math.Min(military.BaseScore * militarySeverity, 100.0) * (double)military.Weight, 0.0001);

            // Total rounds to 2dp and clamps to [1,100]
            var totalWeighted = result.Contributions.Sum(c => c.WeightedContribution);
            var expected = Math.Clamp(Math.Round(totalWeighted, 2), 1.0, 100.0);
            result.Score.Should().BeApproximately(expected, 0.001);
            result.Level.Should().Be(Domain.Enums.RiskLevel.Critical);
        }

        [Fact]
        public void RiskCalculator_ShouldBeDeterministicExceptTimestamp()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new IranRiskTracker.Infrastructure.Storage.InMemoryLiveEventStore();
            var overrideStore = new IranRiskTracker.Infrastructure.Storage.InMemoryOwnerOverrideStore();
            var snapshotStore = new IranRiskTracker.Infrastructure.Storage.InMemoryRiskSnapshotStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore, snapshotStore);

            var a = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            System.Threading.Thread.Sleep(10);
            var b = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();

            // Timestamp may differ
            a.Score.Should().BeApproximately(b.Score, 0.0001);
            a.Level.Should().Be(b.Level);
            a.Summary.Should().Be(b.Summary);
            a.Contributions.Count.Should().Be(b.Contributions.Count);

            for (int i = 0; i < a.Contributions.Count; i++)
            {
                var ca = a.Contributions.ElementAt(i);
                var cb = b.Contributions.ElementAt(i);
                ca.IndicatorKey.Should().Be(cb.IndicatorKey);
                ca.BaseScore.Should().BeApproximately(cb.BaseScore, 0.0001);
                ca.WeightedContribution.Should().BeApproximately(cb.WeightedContribution, 0.0001);
                ca.Explanation.Should().Be(cb.Explanation);
            }
        }
        [Fact]
        public void EventQueryService_CanBeConstructedWithLiveStore()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new IranRiskTracker.Infrastructure.Storage.InMemoryLiveEventStore();
            var sut = new EventQueryService(seed, liveStore);
            sut.Should().NotBeNull();
        }
    }
}
