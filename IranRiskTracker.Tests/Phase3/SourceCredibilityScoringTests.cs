using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using IranRiskTracker.Infrastructure.Seeding;
using IranRiskTracker.Application.Services;
using IranRiskTracker.Infrastructure.Storage;

namespace IranRiskTracker.Tests.Phase3
{
    public class SourceCredibilityScoringTests
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
        public void LiveEvent_WithSeedSourceName_UsesSeedCredibility()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var sources = seed.GetSources().ToList();
            sources.Should().NotBeEmpty();

            var seedSource = sources.First();

            var liveStore = new InMemoryLiveEventStore();
            var qsvc = new EventQueryService(seed, liveStore);
            var overrideStore = new IranRiskTracker.Infrastructure.Storage.InMemoryOwnerOverrideStore();
            var snapshotStore = new IranRiskTracker.Infrastructure.Storage.InMemoryRiskSnapshotStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore, snapshotStore);

            // Exact name match
            var evt = new IranRiskTracker.Application.DTOs.LiveEventCreateRequest
            {
                Title = "seeded",
                RawContent = "r",
                SourceName = seedSource.Name,
                OccurredAt = DateTime.UtcNow,
                Category = Domain.Enums.EventCategory.Cyber,
                Urgency = Domain.Enums.UrgencyLevel.High
            };

            qsvc.AcceptLiveEvent(evt);

            var res = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            var cyber = res.Contributions.Single(c => c.IndicatorKey == "cyber_incidents");
            cyber.LiveSignals.Should().HaveCount(1);

            var sig = cyber.LiveSignals.Single();
            sig.SourceMatchedFromSeed.Should().BeTrue();
            sig.SourceCredibility.Should().Be((double)seedSource.Credibility.Value);
            sig.CredibilityAdjustedUrgencyScore.Should().BeApproximately(sig.UrgencyScore * (double)seedSource.Credibility.Value, 0.0001);
            // Recent event recency multiplier should be 1.0
            sig.RecencyMultiplier.Should().Be(1.0);
            sig.RecencyAdjustedUrgencyScore.Should().BeApproximately(sig.CredibilityAdjustedUrgencyScore * sig.RecencyMultiplier, 0.0001);
        }

        [Fact]
        public void LiveEvent_WithSeedSourceName_DifferentCasing_StillMatches()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var sources = seed.GetSources().ToList();
            sources.Should().NotBeEmpty();

            var seedSource = sources.First();

            var liveStore = new InMemoryLiveEventStore();
            var qsvc = new EventQueryService(seed, liveStore);
            var overrideStore = new IranRiskTracker.Infrastructure.Storage.InMemoryOwnerOverrideStore();
            var snapshotStore = new IranRiskTracker.Infrastructure.Storage.InMemoryRiskSnapshotStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore, snapshotStore);

            // Use different casing for the source name
            var mixedCase = seedSource.Name.ToUpperInvariant();

            var evt = new IranRiskTracker.Application.DTOs.LiveEventCreateRequest
            {
                Title = "seeded-case",
                RawContent = "r",
                SourceName = mixedCase,
                OccurredAt = DateTime.UtcNow,
                Category = Domain.Enums.EventCategory.Cyber,
                Urgency = Domain.Enums.UrgencyLevel.High
            };

            qsvc.AcceptLiveEvent(evt);

            var res = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            var cyber = res.Contributions.Single(c => c.IndicatorKey == "cyber_incidents");
            cyber.LiveSignals.Should().HaveCount(1);

            var sig = cyber.LiveSignals.Single();
            sig.SourceMatchedFromSeed.Should().BeTrue();
            sig.SourceCredibility.Should().Be((double)seedSource.Credibility.Value);
            sig.CredibilityAdjustedUrgencyScore.Should().BeApproximately(sig.UrgencyScore * (double)seedSource.Credibility.Value, 0.0001);
            sig.RecencyMultiplier.Should().Be(1.0);
            sig.RecencyAdjustedUrgencyScore.Should().BeApproximately(sig.CredibilityAdjustedUrgencyScore * sig.RecencyMultiplier, 0.0001);
        }
    }
}
