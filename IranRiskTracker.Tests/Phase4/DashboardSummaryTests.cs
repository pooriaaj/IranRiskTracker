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
        public async Task Service_Returns_ValidSummary()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var overrideStore = new InMemoryOwnerOverrideStore();
            var snapshotStore = new InMemoryRiskSnapshotStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore, snapshotStore);
            var svc = new DashboardSummaryService(calc);

            var dto = await svc.GetSummaryAsync();

            dto.ScorePercent.Should().BeInRange(1, 100);
            dto.Score.Should().BeInRange(1.0, 100.0);
            Enum.IsDefined(typeof(IranRiskTracker.Domain.Enums.RiskLevel), dto.Level).Should().BeTrue();
            dto.ScoreTrend.Should().Be("NoPreviousSnapshot");
            dto.PreviousScore.Should().BeNull();
        }

        [Fact]
        public async Task TopContributors_LimitedAndOrdered()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var overrideStore = new InMemoryOwnerOverrideStore();
            var snapshotStore = new InMemoryRiskSnapshotStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore, snapshotStore);
            var svc = new DashboardSummaryService(calc);

            var dto = await svc.GetSummaryAsync();

            dto.TopContributors.Count.Should().BeLessOrEqualTo(5);
            var ordered = dto.TopContributors.OrderByDescending(t => t.WeightedContribution).ToList();
            dto.TopContributors.Should().BeEquivalentTo(ordered, opts => opts.WithStrictOrdering());
        }

        [Fact]
        public async Task Controller_Returns_Ok()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var overrideStore = new InMemoryOwnerOverrideStore();
            var snapshotStore = new InMemoryRiskSnapshotStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore, snapshotStore);
            var svc = new DashboardSummaryService(calc);
            var controller = new IranRiskTracker.Api.Controllers.DashboardController(svc);

            var res = await controller.GetSummary();
            res.Should().BeOfType<OkObjectResult>();
            var ok = res as OkObjectResult;
            ok!.Value.Should().BeOfType<IranRiskTracker.Application.DTOs.DashboardSummaryDto>();
        }
    }
}
