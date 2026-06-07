using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using IranRiskTracker.Infrastructure.Seeding;
using IranRiskTracker.Application.Services;
using IranRiskTracker.Infrastructure.Storage;

namespace IranRiskTracker.Tests.Phase3
{
    public class CorroborationScoringTests
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
        public void OneLiveSource_GivesMultiplierOne()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var qsvc = new EventQueryService(seed, liveStore);
            var overrideStore = new InMemoryOwnerOverrideStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore);

            qsvc.AcceptLiveEvent(new IranRiskTracker.Application.DTOs.LiveEventCreateRequest { Title = "a", RawContent = "r", SourceName = "one", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Cyber, Urgency = Domain.Enums.UrgencyLevel.High });

            var res = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            var cyber = res.Contributions.Single(c => c.IndicatorKey == "cyber_incidents");

            cyber.DistinctLiveSourceCount.Should().Be(1);
            cyber.CorroborationMultiplier.Should().Be(1.0);
        }

        [Fact]
        public void TwoDistinctLiveSources_GivesMultiplier110()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var qsvc = new EventQueryService(seed, liveStore);
            var overrideStore = new InMemoryOwnerOverrideStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore);

            qsvc.AcceptLiveEvent(new IranRiskTracker.Application.DTOs.LiveEventCreateRequest { Title = "a", RawContent = "r", SourceName = "s1", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Cyber, Urgency = Domain.Enums.UrgencyLevel.Medium });
            qsvc.AcceptLiveEvent(new IranRiskTracker.Application.DTOs.LiveEventCreateRequest { Title = "b", RawContent = "r", SourceName = "s2", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Cyber, Urgency = Domain.Enums.UrgencyLevel.Medium });

            var res = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            var cyber = res.Contributions.Single(c => c.IndicatorKey == "cyber_incidents");

            cyber.DistinctLiveSourceCount.Should().Be(2);
            cyber.CorroborationMultiplier.Should().Be(1.10);
        }

        [Fact]
        public void SameSourceDifferentCasing_CountsAsOne()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var qsvc = new EventQueryService(seed, liveStore);
            var overrideStore = new InMemoryOwnerOverrideStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore);

            qsvc.AcceptLiveEvent(new IranRiskTracker.Application.DTOs.LiveEventCreateRequest { Title = "a", RawContent = "r", SourceName = "Case", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Cyber, Urgency = Domain.Enums.UrgencyLevel.Medium });
            qsvc.AcceptLiveEvent(new IranRiskTracker.Application.DTOs.LiveEventCreateRequest { Title = "b", RawContent = "r", SourceName = "case", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Cyber, Urgency = Domain.Enums.UrgencyLevel.Medium });

            var res = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            var cyber = res.Contributions.Single(c => c.IndicatorKey == "cyber_incidents");

            cyber.DistinctLiveSourceCount.Should().Be(1);
            cyber.CorroborationMultiplier.Should().Be(1.0);
        }

        [Fact]
        public void ThreeDistinctSources_GivesMultiplier120()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var qsvc = new EventQueryService(seed, liveStore);
            var overrideStore = new InMemoryOwnerOverrideStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore);

            qsvc.AcceptLiveEvent(new IranRiskTracker.Application.DTOs.LiveEventCreateRequest { Title = "a", RawContent = "r", SourceName = "s1", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Cyber, Urgency = Domain.Enums.UrgencyLevel.Medium });
            qsvc.AcceptLiveEvent(new IranRiskTracker.Application.DTOs.LiveEventCreateRequest { Title = "b", RawContent = "r", SourceName = "s2", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Cyber, Urgency = Domain.Enums.UrgencyLevel.Medium });
            qsvc.AcceptLiveEvent(new IranRiskTracker.Application.DTOs.LiveEventCreateRequest { Title = "c", RawContent = "r", SourceName = "s3", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Cyber, Urgency = Domain.Enums.UrgencyLevel.Medium });

            var res = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            var cyber = res.Contributions.Single(c => c.IndicatorKey == "cyber_incidents");

            cyber.DistinctLiveSourceCount.Should().Be(3);
            cyber.CorroborationMultiplier.Should().Be(1.20);
        }

        [Fact]
        public void FourOrMoreDistinctSources_CapsAt130()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var qsvc = new EventQueryService(seed, liveStore);
            var overrideStore = new InMemoryOwnerOverrideStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore);

            // add five distinct sources to ensure cap
            qsvc.AcceptLiveEvent(new IranRiskTracker.Application.DTOs.LiveEventCreateRequest { Title = "a", RawContent = "r", SourceName = "s1", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Cyber, Urgency = Domain.Enums.UrgencyLevel.Medium });
            qsvc.AcceptLiveEvent(new IranRiskTracker.Application.DTOs.LiveEventCreateRequest { Title = "b", RawContent = "r", SourceName = "s2", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Cyber, Urgency = Domain.Enums.UrgencyLevel.Medium });
            qsvc.AcceptLiveEvent(new IranRiskTracker.Application.DTOs.LiveEventCreateRequest { Title = "c", RawContent = "r", SourceName = "s3", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Cyber, Urgency = Domain.Enums.UrgencyLevel.Medium });
            qsvc.AcceptLiveEvent(new IranRiskTracker.Application.DTOs.LiveEventCreateRequest { Title = "d", RawContent = "r", SourceName = "s4", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Cyber, Urgency = Domain.Enums.UrgencyLevel.Medium });
            qsvc.AcceptLiveEvent(new IranRiskTracker.Application.DTOs.LiveEventCreateRequest { Title = "e", RawContent = "r", SourceName = "s5", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Cyber, Urgency = Domain.Enums.UrgencyLevel.Medium });

            var res = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            var cyber = res.Contributions.Single(c => c.IndicatorKey == "cyber_incidents");

            cyber.DistinctLiveSourceCount.Should().BeGreaterOrEqualTo(4);
            cyber.CorroborationMultiplier.Should().Be(1.30);
        }

        [Fact]
        public void LiveBaseScore_IsSumRecencyAdjustedTimesMultiplier()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var qsvc = new EventQueryService(seed, liveStore);
            var overrideStore = new InMemoryOwnerOverrideStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore);

            qsvc.AcceptLiveEvent(new IranRiskTracker.Application.DTOs.LiveEventCreateRequest { Title = "a", RawContent = "r", SourceName = "s1", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Cyber, Urgency = Domain.Enums.UrgencyLevel.Medium });
            qsvc.AcceptLiveEvent(new IranRiskTracker.Application.DTOs.LiveEventCreateRequest { Title = "b", RawContent = "r", SourceName = "s2", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Cyber, Urgency = Domain.Enums.UrgencyLevel.High });

            var res = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            var cyber = res.Contributions.Single(c => c.IndicatorKey == "cyber_incidents");

            var sumRecency = cyber.LiveSignals.Sum(s => s.RecencyAdjustedUrgencyScore);
            cyber.LiveBaseScore.Should().BeApproximately(sumRecency * cyber.CorroborationMultiplier, 0.0001);
        }
    }
}
