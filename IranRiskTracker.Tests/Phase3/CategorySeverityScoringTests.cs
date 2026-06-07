using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using IranRiskTracker.Infrastructure.Seeding;
using IranRiskTracker.Application.Services;
using IranRiskTracker.Infrastructure.Storage;

namespace IranRiskTracker.Tests.Phase3
{
    public class CategorySeverityScoringTests
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
        public void CyberIndicator_UsesSeverityMultiplier_105()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var overrideStore = new InMemoryOwnerOverrideStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore);

            var res = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            var cyber = res.Contributions.Single(c => c.IndicatorKey == "cyber_incidents");

            cyber.CategorySeverityMultiplier.Should().Be(1.05);
            cyber.SeverityAdjustedBaseScore.Should().BeApproximately(Math.Clamp(cyber.BaseScore * 1.05, 0.0, 100.0), 0.0001);
            cyber.WeightedContribution.Should().BeApproximately(cyber.SeverityAdjustedBaseScore * (double)cyber.Weight * 1, 0.0001);
            cyber.WeightedContribution.Should().BeGreaterOrEqualTo(0.0);
            cyber.Explanation.Should().Contain("CategorySeverityMultiplier");
            cyber.Explanation.Should().Contain("SeverityAdjustedBaseScore");
        }

        [Fact]
        public void MilitaryIndicator_UsesSeverityMultiplier_130()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var overrideStore = new InMemoryOwnerOverrideStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore);

            var res = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            var military = res.Contributions.Single(c => c.IndicatorKey == "military_activity");

            military.CategorySeverityMultiplier.Should().Be(1.30);
            military.SeverityAdjustedBaseScore.Should().BeApproximately(Math.Clamp(military.BaseScore * 1.30, 0.0, 100.0), 0.0001);
            military.WeightedContribution.Should().BeApproximately(military.SeverityAdjustedBaseScore * (double)military.Weight * 1, 0.0001);
        }

        [Fact]
        public void PoliticalIndicator_UsesSeverityMultiplier_090()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var overrideStore = new InMemoryOwnerOverrideStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore);

            var res = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            var political = res.Contributions.Single(c => c.IndicatorKey == "political_instability");

            political.CategorySeverityMultiplier.Should().Be(0.90);
            political.SeverityAdjustedBaseScore.Should().BeApproximately(Math.Clamp(political.BaseScore * 0.90, 0.0, 100.0), 0.0001);
            political.WeightedContribution.Should().BeApproximately(political.SeverityAdjustedBaseScore * (double)political.Weight * 1, 0.0001);
        }

        [Fact]
        public void ExecutionsIndicator_UsesSeverityMultiplier_120()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var overrideStore = new InMemoryOwnerOverrideStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore);

            var res = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            var exec = res.Contributions.Single(c => c.IndicatorKey == "executions_repression");

            exec.CategorySeverityMultiplier.Should().Be(1.20);
            exec.SeverityAdjustedBaseScore.Should().BeApproximately(Math.Clamp(exec.BaseScore * 1.20, 0.0, 100.0), 0.0001);
            exec.WeightedContribution.Should().BeApproximately(exec.SeverityAdjustedBaseScore * (double)exec.Weight * 1, 0.0001);
        }

        [Fact]
        public void OwnerOverrides_StillApply_AfterSeverityAdjustment()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var qsvc = new EventQueryService(seed, liveStore);
            var overrideStore = new InMemoryOwnerOverrideStore();
            var svc = new IranRiskTracker.Application.Services.OwnerOverrideService(overrideStore);

            var calc = new RiskCalculator(seed, liveStore, overrideStore);
            var before = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();

            // add live event to ensure non-zero base change
            qsvc.AcceptLiveEvent(new IranRiskTracker.Application.DTOs.LiveEventCreateRequest { Title = "x", RawContent = "r", SourceName = "s", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Cyber, Urgency = Domain.Enums.UrgencyLevel.High });

            svc.Add(new IranRiskTracker.Application.DTOs.OwnerOverrideCreateRequest { Title = "o", Reasoning = "r", Category = Domain.Enums.EventCategory.Cyber, ScoreAdjustment = 5.0, AppliedAt = DateTime.UtcNow });

            var after = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();

            (after.Score - before.Score).Should().BeApproximately((after.BaseScoreBeforeOverrides - before.BaseScoreBeforeOverrides) + 5.0, 0.0001);
        }
    }
}
