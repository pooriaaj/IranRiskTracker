using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using IranRiskTracker.Infrastructure.Seeding;
using IranRiskTracker.Application.Services;
using IranRiskTracker.Infrastructure.Storage;
using IranRiskTracker.Application.DTOs;

namespace IranRiskTracker.Tests.Phase2
{
    public class LiveSignalTracingTests
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
        public void EmptyLiveStore_ProducesEmptyLiveSignals()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var overrideStore = new IranRiskTracker.Infrastructure.Storage.InMemoryOwnerOverrideStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore);

            var res = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            foreach (var c in res.Contributions)
            {
                c.LiveSignals.Should().BeEmpty();
            }
        }

        [Fact]
        public void HighCyberLiveEvent_AppearsInCyberContributionOnly()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var qsvc = new EventQueryService(seed, liveStore);
            var overrideStore = new IranRiskTracker.Infrastructure.Storage.InMemoryOwnerOverrideStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore);

            var evt = new LiveEventCreateRequest { Title = "x", RawContent = "r", SourceName = "s", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Cyber, Urgency = Domain.Enums.UrgencyLevel.High };
            qsvc.AcceptLiveEvent(evt);

            var res = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            var cyber = res.Contributions.Single(c => c.IndicatorKey == "cyber_incidents");
            cyber.LiveSignals.Should().HaveCount(1);

            // Other contributions should have none
            foreach (var c in res.Contributions.Where(c => c.IndicatorKey != "cyber_incidents"))
            {
                c.LiveSignals.Should().BeEmpty();
            }
        }

        [Fact]
        public void LiveSignalDto_IncludesSourceMetadata_AndUrgencyScore()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var qsvc = new EventQueryService(seed, liveStore);
            var overrideStore = new IranRiskTracker.Infrastructure.Storage.InMemoryOwnerOverrideStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore);

            var evt = new LiveEventCreateRequest { Title = "x", RawContent = "r", SourceName = "src", SourceUrl = "https://x", SourceHandle = "@h", OwnerNotes = "n", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Cyber, Urgency = Domain.Enums.UrgencyLevel.High };
            qsvc.AcceptLiveEvent(evt);

            var res = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            var cyber = res.Contributions.Single(c => c.IndicatorKey == "cyber_incidents");
            var sig = cyber.LiveSignals.Single();

            sig.SourceName.Should().Be("src");
            sig.SourceUrl.Should().Be("https://x");
            sig.SourceHandle.Should().Be("@h");
            sig.OwnerNotes.Should().Be("n");
            sig.UrgencyScore.Should().Be(20.0);
            // Default src is not in seed data so should be unmatched and use default credibility 0.5
            sig.SourceMatchedFromSeed.Should().BeFalse();
            sig.SourceCredibility.Should().Be(0.5);
            sig.CredibilityAdjustedUrgencyScore.Should().Be(10.0);
            // Recent event default should use recency multiplier 1.0 and hence same as credibility-adjusted
            sig.RecencyMultiplier.Should().Be(1.0);
            sig.RecencyAdjustedUrgencyScore.Should().Be(10.0);
        }

        [Fact]
        public void LiveBaseScore_EqualsSumOfLiveSignalUrgencyScores()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var qsvc = new EventQueryService(seed, liveStore);
            var overrideStore = new IranRiskTracker.Infrastructure.Storage.InMemoryOwnerOverrideStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore);

            qsvc.AcceptLiveEvent(new LiveEventCreateRequest { Title = "a", RawContent = "r", SourceName = "s", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Cyber, Urgency = Domain.Enums.UrgencyLevel.Medium });
            qsvc.AcceptLiveEvent(new LiveEventCreateRequest { Title = "b", RawContent = "r", SourceName = "s", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Cyber, Urgency = Domain.Enums.UrgencyLevel.High });

            var res = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            var cyber = res.Contributions.Single(c => c.IndicatorKey == "cyber_incidents");

            var expected = cyber.LiveSignals.Sum(s => s.RecencyAdjustedUrgencyScore);
            cyber.LiveBaseScore.Should().BeApproximately(expected, 0.0001);
        }
    }
}
