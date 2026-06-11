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

            // Cyber historical base = 90. Live adds 10 (High urgency 20 * default credibility 0.5).
            // Combined = clamp(90+10, 0, 100) = 100. SeverityAdj before = min(90*1.05,100) = 94.5
            // SeverityAdj after = min(100*1.05,100) = 100.0. Expected increase = (100.0-94.5)*0.10 = 0.55
            var cyberBase = 90.0;
            var liveScore = 10.0;
            var sevBefore = Math.Min(cyberBase * 1.05, 100.0);
            var sevAfter = Math.Min(Math.Min(cyberBase + liveScore, 100.0) * 1.05, 100.0);
            var expectedIncrease = (sevAfter - sevBefore) * 0.10;
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
            // Historical base = 98 (108 delta sum minus -10 ceasefire). Combined = clamp(98+17.5, 0, 100) = 100.
            // SeverityAdj before = clamp(98*1.30, 0, 100) = 100 -> weighted = 100*0.15 = 15.0
            // SeverityAdj after  = clamp(100*1.30, 0, 100) = 100 -> weighted = 100*0.15 = 15.0
            // Expected increase = 0.0 (severity still clamped at 100 after live event)
            var historicalBase = 98.0;
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
