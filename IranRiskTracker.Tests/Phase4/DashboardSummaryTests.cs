using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using IranRiskTracker.Infrastructure.Seeding;
using IranRiskTracker.Infrastructure.Storage;
using IranRiskTracker.Application.Services;
using IranRiskTracker.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IranRiskTracker.Tests.Phase4
{
    public class DashboardSummaryTests
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
        public void Service_Returns_ValidSummary()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var overrideStore = new InMemoryOwnerOverrideStore();
            var snapshotStore = new InMemoryRiskSnapshotStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore, snapshotStore);
            var svc = new DashboardSummaryService(calc);

            var dto = svc.GetSummaryAsync().GetAwaiter().GetResult();

            dto.ScorePercent.Should().BeInRange(1, 100);
            dto.Level.Should().Be(calc.GetCurrentRiskAsync().GetAwaiter().GetResult().Level);
            dto.ScoreTrend.Should().NotBeNull();
            dto.PreviousScore.Should().BeNull();
        }

        [Fact]
        public void TopContributors_LimitedAndOrdered()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var overrideStore = new InMemoryOwnerOverrideStore();
            var snapshotStore = new InMemoryRiskSnapshotStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore, snapshotStore);
            var svc = new DashboardSummaryService(calc);

            var dto = svc.GetSummaryAsync().GetAwaiter().GetResult();

            dto.TopContributors.Count.Should().BeLessOrEqualTo(5);
            var ordered = dto.TopContributors.OrderByDescending(t => t.WeightedContribution).ToList();
            dto.TopContributors.Should().BeEquivalentTo(ordered, opts => opts.WithStrictOrdering());
        }

        [Fact]
        public void Controller_Returns_Ok()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var overrideStore = new InMemoryOwnerOverrideStore();
            var snapshotStore = new InMemoryRiskSnapshotStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore, snapshotStore);
            var svc = new DashboardSummaryService(calc);
            var controller = new IranRiskTracker.Api.Controllers.DashboardController(svc);

            var res = controller.GetSummary().GetAwaiter().GetResult();
            res.Should().BeOfType<OkObjectResult>();
        }
    }
}
