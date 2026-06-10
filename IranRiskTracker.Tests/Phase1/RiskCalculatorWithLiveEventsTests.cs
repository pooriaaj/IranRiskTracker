using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using IranRiskTracker.Infrastructure.Seeding;
using IranRiskTracker.Application.Services;
using IranRiskTracker.Infrastructure.Storage;

namespace IranRiskTracker.Tests.Phase1
{
    public class RiskCalculatorWithLiveEventsTests
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
        public void HighCyberLiveEvent_IncreasesCyberContribution()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var overrideStore = new IranRiskTracker.Infrastructure.Storage.InMemoryOwnerOverrideStore();
            var snapshotStore = new IranRiskTracker.Infrastructure.Storage.InMemoryRiskSnapshotStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore, snapshotStore);

            // Ensure baseline
            var before = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            var cyber = before.Contributions.Single(c => c.IndicatorKey == "cyber_incidents");

            // Add a High urgency live cyber event
            var now = DateTime.UtcNow;
            var evt = new IranRiskTracker.Application.DTOs.LiveEventCreateRequest { Title = "x", RawContent = "r", SourceName = "s", OccurredAt = now, Category = Domain.Enums.EventCategory.Cyber, Urgency = Domain.Enums.UrgencyLevel.High };
            var qsvc = new EventQueryService(seed, liveStore);
            qsvc.AcceptLiveEvent(evt);

            var after = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            var cyberAfter = after.Contributions.Single(c => c.IndicatorKey == "cyber_incidents");

            // Historical base score unchanged; live base score should add LiveUrgencyHigh (20.0) multiplied by indicator weight 0.10
            // Source 's' is not in seed data, default credibility 0.5 applies -> effective urgency = 10.0
            // Category severity multiplier for Cyber = 1.05 -> effective increase = 10.0 * 1.05 * 0.10
            var expectedIncrease = 10.0 * 1.05 * 0.10; // 1.05
            (cyberAfter.WeightedContribution - cyber.WeightedContribution).Should().BeApproximately(expectedIncrease, 0.0001);
        }

        [Fact]
        public void CriticalMilitaryLiveEvent_IncreasesMilitaryContribution()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var overrideStore = new IranRiskTracker.Infrastructure.Storage.InMemoryOwnerOverrideStore();
            var snapshotStore = new IranRiskTracker.Infrastructure.Storage.InMemoryRiskSnapshotStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore, snapshotStore);

            var before = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            var military = before.Contributions.Single(c => c.IndicatorKey == "military_activity");

            var evt = new IranRiskTracker.Application.DTOs.LiveEventCreateRequest { Title = "m", RawContent = "r", SourceName = "s", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Military, Urgency = Domain.Enums.UrgencyLevel.Critical };
            var qsvc = new EventQueryService(seed, liveStore);
            qsvc.AcceptLiveEvent(evt);

            var after = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            var militaryAfter = after.Contributions.Single(c => c.IndicatorKey == "military_activity");

            // Source 's' not in seed: default credibility 0.5, recency 1.0 -> live urgency = 35*0.5 = 17.5
            // Historical base = 75, combined = 75+17.5 = 92.5
            // Severity-adjusted = clamp(92.5*1.30, 0, 100) = clamp(120.25, 0, 100) = 100 -> weighted = 100*0.15 = 15.0
            // Before: clamp(75*1.30, 0, 100) = 97.5 -> 97.5*0.15 = 14.625
            // Expected increase = 15.0 - 14.625 = 0.375
            var historicalBase = 75.0;
            var liveUrgency = 17.5;
            var combined = Math.Min(historicalBase + liveUrgency, 100.0);
            var severityAfter = Math.Min(combined * 1.30, 100.0);
            var severityBefore = Math.Min(historicalBase * 1.30, 100.0);
            var expectedIncrease = (severityAfter - severityBefore) * 0.15;
            (militaryAfter.WeightedContribution - military.WeightedContribution).Should().BeApproximately(expectedIncrease, 0.0001);
        }

        [Fact]
        public void ScoreRemainsDeterministicExceptTimestamp()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var overrideStore = new IranRiskTracker.Infrastructure.Storage.InMemoryOwnerOverrideStore();
            var snapshotStore = new IranRiskTracker.Infrastructure.Storage.InMemoryRiskSnapshotStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore, snapshotStore);

            var a = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            System.Threading.Thread.Sleep(10);
            var b = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();

            a.Score.Should().BeApproximately(b.Score, 0.0001);
            a.Level.Should().Be(b.Level);
            a.Summary.Should().Be(b.Summary);
        }
    }
}
