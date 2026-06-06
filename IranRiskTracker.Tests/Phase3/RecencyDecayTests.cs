using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using IranRiskTracker.Infrastructure.Seeding;
using IranRiskTracker.Application.Services;
using IranRiskTracker.Infrastructure.Storage;

namespace IranRiskTracker.Tests.Phase3
{
    public class RecencyDecayTests
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

        [Theory]
        [InlineData(2, 1.0)]
        [InlineData(12, 0.75)]
        [InlineData(48, 0.50)]
        [InlineData(96, 0.25)]
        public void RecencyMultiplier_Applies_ByAgeHours(int hoursAgo, double expectedMultiplier)
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var sources = seed.GetSources().ToList();
            sources.Should().NotBeEmpty();

            var seedSource = sources.First();

            var liveStore = new InMemoryLiveEventStore();
            var qsvc = new EventQueryService(seed, liveStore);
            var overrideStore = new IranRiskTracker.Infrastructure.Storage.InMemoryOwnerOverrideStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore);

            var occurred = DateTime.UtcNow.AddHours(-hoursAgo);

            var evt = new IranRiskTracker.Application.DTOs.LiveEventCreateRequest
            {
                Title = "recency",
                RawContent = "r",
                SourceName = seedSource.Name,
                OccurredAt = occurred,
                Category = Domain.Enums.EventCategory.Cyber,
                Urgency = Domain.Enums.UrgencyLevel.High
            };

            qsvc.AcceptLiveEvent(evt);

            var res = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            var cyber = res.Contributions.Single(c => c.IndicatorKey == "cyber_incidents");
            cyber.LiveSignals.Should().HaveCount(1);

            var sig = cyber.LiveSignals.Single();
            sig.RecencyMultiplier.Should().Be(expectedMultiplier);
            sig.RecencyAdjustedUrgencyScore.Should().BeApproximately(sig.CredibilityAdjustedUrgencyScore * expectedMultiplier, 0.0001);
        }

        [Fact]
        public void LiveBaseScore_EqualsSumOfRecencyAdjustedScores()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var sources = seed.GetSources().ToList();
            sources.Should().NotBeEmpty();

            var seedSource = sources.First();

            var liveStore = new InMemoryLiveEventStore();
            var qsvc = new EventQueryService(seed, liveStore);
            var overrideStore = new IranRiskTracker.Infrastructure.Storage.InMemoryOwnerOverrideStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore);

            qsvc.AcceptLiveEvent(new IranRiskTracker.Application.DTOs.LiveEventCreateRequest { Title = "a", RawContent = "r", SourceName = seedSource.Name, OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Cyber, Urgency = Domain.Enums.UrgencyLevel.Medium });
            qsvc.AcceptLiveEvent(new IranRiskTracker.Application.DTOs.LiveEventCreateRequest { Title = "b", RawContent = "r", SourceName = seedSource.Name, OccurredAt = DateTime.UtcNow.AddHours(-12), Category = Domain.Enums.EventCategory.Cyber, Urgency = Domain.Enums.UrgencyLevel.High });

            var res = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            var cyber = res.Contributions.Single(c => c.IndicatorKey == "cyber_incidents");

            var expected = cyber.LiveSignals.Sum(s => s.RecencyAdjustedUrgencyScore);
            cyber.LiveBaseScore.Should().BeApproximately(expected, 0.0001);
        }

        [Fact]
        public void OwnerOverrides_StillApply_AfterRecencyAdjustment()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var sources = seed.GetSources().ToList();
            sources.Should().NotBeEmpty();

            var seedSource = sources.First();

            var liveStore = new InMemoryLiveEventStore();
            var qsvc = new EventQueryService(seed, liveStore);
            var overrideStore = new IranRiskTracker.Infrastructure.Storage.InMemoryOwnerOverrideStore();
            var svc = new IranRiskTracker.Application.Services.OwnerOverrideService(overrideStore);

            // baseline
            var calc = new RiskCalculator(seed, liveStore, overrideStore);
            var before = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            var cyberBefore = before.Contributions.Single(c => c.IndicatorKey == "cyber_incidents");

            // add live event
            qsvc.AcceptLiveEvent(new IranRiskTracker.Application.DTOs.LiveEventCreateRequest { Title = "x", RawContent = "r", SourceName = seedSource.Name, OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Cyber, Urgency = Domain.Enums.UrgencyLevel.High });

            // apply override
            svc.Add(new IranRiskTracker.Application.DTOs.OwnerOverrideCreateRequest { Title = "o", Reasoning = "r", Category = Domain.Enums.EventCategory.Cyber, ScoreAdjustment = 5.0, AppliedAt = DateTime.UtcNow });

            var after = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            var cyberAfter = after.Contributions.Single(c => c.IndicatorKey == "cyber_incidents");

            // Ensure override reflected in final score difference
            var baseBefore = before.BaseScoreBeforeOverrides;
            var baseAfter = after.BaseScoreBeforeOverrides;
            (after.Score - before.Score).Should().BeApproximately((baseAfter - baseBefore) + 5.0, 0.0001);
        }
    }
}
