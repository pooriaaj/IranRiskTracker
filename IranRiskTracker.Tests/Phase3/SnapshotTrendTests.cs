using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using IranRiskTracker.Infrastructure.Seeding;
using IranRiskTracker.Application.Services;
using IranRiskTracker.Infrastructure.Storage;
using IranRiskTracker.Application.DTOs;

namespace IranRiskTracker.Tests.Phase3
{
    public class SnapshotTrendTests
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
        public void FirstCalculation_HasNoPreviousSnapshot()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var overrideStore = new InMemoryOwnerOverrideStore();
            var snapshotStore = new InMemoryRiskSnapshotStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore, snapshotStore);

            var first = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();

            first.PreviousScore.Should().BeNull();
            first.ScoreChange.Should().BeApproximately(0.0, 0.0001);
            first.ScoreTrend.Should().Be("NoPreviousSnapshot");
        }

        [Fact]
        public void SecondCalculation_WithNoChanges_IsUnchanged()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var overrideStore = new InMemoryOwnerOverrideStore();
            var snapshotStore = new InMemoryRiskSnapshotStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore, snapshotStore);

            var first = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            var second = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();

            second.PreviousScore.Should().BeApproximately(first.Score, 0.0001);
            second.ScoreChange.Should().BeApproximately(0.0, 0.0001);
            second.ScoreTrend.Should().Be("Unchanged");
        }

        [Fact]
        public void SecondCalculation_WhenIncreased_IsMarkedIncreased()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var overrideStore = new InMemoryOwnerOverrideStore();
            var snapshotStore = new InMemoryRiskSnapshotStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore, snapshotStore);
            var svc = new IranRiskTracker.Application.Services.OwnerOverrideService(overrideStore);

            var first = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();

            // Add a positive override to increase final score
            svc.Add(new IranRiskTracker.Application.DTOs.OwnerOverrideCreateRequest { Title = "up", Reasoning = "r", Category = Domain.Enums.EventCategory.Cyber, ScoreAdjustment = 5.0, AppliedAt = DateTime.UtcNow });

            var second = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();

            second.PreviousScore.Should().BeApproximately(first.Score, 0.0001);
            second.ScoreChange.Should().BeGreaterThan(0.0);
            second.ScoreTrend.Should().Be("Increased");
        }

        [Fact]
        public void SecondCalculation_WhenDecreased_IsMarkedDecreased()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var overrideStore = new InMemoryOwnerOverrideStore();
            var snapshotStore = new InMemoryRiskSnapshotStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore, snapshotStore);
            var svc = new IranRiskTracker.Application.Services.OwnerOverrideService(overrideStore);

            var first = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();

            // Add a negative override to decrease final score
            svc.Add(new IranRiskTracker.Application.DTOs.OwnerOverrideCreateRequest { Title = "down", Reasoning = "r", Category = Domain.Enums.EventCategory.Cyber, ScoreAdjustment = -5.0, AppliedAt = DateTime.UtcNow });

            var second = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();

            second.PreviousScore.Should().BeApproximately(first.Score, 0.0001);
            second.ScoreChange.Should().BeLessThan(0.0);
            second.ScoreTrend.Should().Be("Decreased");
        }

        [Fact]
        public void SnapshotStore_GetAll_ReturnsNewestFirst()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var overrideStore = new InMemoryOwnerOverrideStore();
            var snapshotStore = new InMemoryRiskSnapshotStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore, snapshotStore);

            var first = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();
            System.Threading.Thread.Sleep(10); // ensure timestamp order
            var second = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();

            var all = snapshotStore.GetAll().ToList();
            all.Should().HaveCountGreaterOrEqualTo(2);
            all.First().Timestamp.Should().BeOnOrAfter(all.Skip(1).First().Timestamp);
        }
    }
}
