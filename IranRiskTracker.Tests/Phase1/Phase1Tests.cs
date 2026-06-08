using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using IranRiskTracker.Infrastructure.Seeding;
using IranRiskTracker.Application.Services;
using IranRiskTracker.Application.Interfaces;
using IranRiskTracker.Application.DTOs;

namespace IranRiskTracker.Tests.Phase1
{
    /// <summary>
    /// Phase 1 verification tests ensuring the seed-driven backend surface is wired correctly.
    /// These tests are intentionally integration-light and do not require a database.
    /// </summary>
    public class Phase1Tests
    {
        private static string FindSeedDataPath()
        {
            // Walk upwards from the current directory until we find the Infrastructure/Seeding/Data folder.
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "IranRiskTracker.Infrastructure", "Seeding", "Data");
                if (Directory.Exists(candidate)) return candidate;
                dir = dir.Parent;
            }

            throw new DirectoryNotFoundException("Could not locate Seeding/Data folder for tests.");
        }

        [Fact]
        public void JsonSeedDataProvider_ShouldLoadSeedFiles()
        {
            // Arrange
            var basePath = FindSeedDataPath();
            var provider = new JsonSeedDataProvider(basePath);

            // Act
            var events = provider.GetHistoricalEvents().ToList();
            var indicators = provider.GetIndicators().ToList();
            var sources = provider.GetSources().ToList();

            // Assert
            events.Should().NotBeNull();
            events.Should().HaveCountGreaterThan(0, "historical seed file should contain at least one event");
            indicators.Should().HaveCount(8, "we expect eight indicators in Phase 1 seed");
            sources.Should().NotBeNull();
        }

        [Fact]
        public void EventQueryService_ShouldReturnHistoricalAndLiveEvents()
        {
            // Arrange
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new IranRiskTracker.Infrastructure.Storage.InMemoryLiveEventStore();
            var sut = new EventQueryService(seed, liveStore);

            // Act
            var historical = sut.GetHistoricalEvents().ToList();
            var live = sut.GetLiveEvents().ToList();

            // Assert
            historical.Should().NotBeEmpty();
            live.Should().BeEmpty("Phase 1 has no persisted live events yet");
            // verify mapping
            var sample = historical.First();
            sample.Title.Should().NotBeNullOrEmpty();
            Assert.True(Enum.IsDefined(typeof(Domain.Enums.EventCategory), sample.Category));
        }

        [Fact]
        public void RiskCalculator_ShouldReturnDeterministicBaseline()
        {
            // Arrange
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new IranRiskTracker.Infrastructure.Storage.InMemoryLiveEventStore();
            var overrideStore = new IranRiskTracker.Infrastructure.Storage.InMemoryOwnerOverrideStore();
            var snapshotStore = new IranRiskTracker.Infrastructure.Storage.InMemoryRiskSnapshotStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore, snapshotStore);

            // Act
            var result = calc.GetCurrentRiskAsync().GetAwaiter().GetResult();

            // Assert
            result.Should().NotBeNull();
            result.Score.Should().BeGreaterThanOrEqualTo(1.0);
            result.Score.Should().BeLessOrEqualTo(100.0);
            Assert.True(Enum.IsDefined(typeof(Domain.Enums.RiskLevel), result.Level));
        }

        [Fact]
        public void EventsController_DelegatesToService()
        {
            // Arrange
            var called = false;
            var fake = new FakeEventQueryService(() => called = true);
            var controller = new IranRiskTracker.Api.Controllers.EventsController(fake);

            // Act
            var res = controller.GetHistorical();

            // Assert delegation and a non-null response -- controller should not perform file IO itself.
            called.Should().BeTrue();
            res.Should().NotBeNull();
        }

        [Fact]
        public void RiskController_ReturnsValidRisk()
        {
            // Arrange
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new IranRiskTracker.Infrastructure.Storage.InMemoryLiveEventStore();
            var overrideStore = new IranRiskTracker.Infrastructure.Storage.InMemoryOwnerOverrideStore();
            var snapshotStoreForRisk = new IranRiskTracker.Infrastructure.Storage.InMemoryRiskSnapshotStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore, snapshotStoreForRisk);
            var controller = new IranRiskTracker.Api.Controllers.RiskController(calc);

            // Act
            var action = controller.GetCurrent().GetAwaiter().GetResult();

            // Assert the controller returns a non-null IActionResult; RiskDto verified by RiskCalculator tests.
            action.Should().NotBeNull();
        }

        [Fact]
        public void SnapshotsController_ReturnsLatest()
        {
            // Arrange
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new IranRiskTracker.Infrastructure.Storage.InMemoryLiveEventStore();
            var overrideStore = new IranRiskTracker.Infrastructure.Storage.InMemoryOwnerOverrideStore();
            var snapshotStore = new IranRiskTracker.Infrastructure.Storage.InMemoryRiskSnapshotStore();
            var calc = new RiskCalculator(seed, liveStore, overrideStore, snapshotStore);
            var controller = new IranRiskTracker.Api.Controllers.SnapshotsController(calc, snapshotStore);

            // Act
            var action = controller.GetLatest().GetAwaiter().GetResult();

            // Assert
            action.Should().NotBeNull();
        }

        /// <summary>
        /// Minimal fake IEventQueryService used to assert controller delegation without file IO.
        /// </summary>
        private class FakeEventQueryService : IEventQueryService
        {
            private readonly Action _onCall;

            public FakeEventQueryService(Action onCall)
            {
                _onCall = onCall;
            }

            public LiveEventDto AcceptLiveEvent(LiveEventCreateRequest request)
            {
                _onCall();
                return new LiveEventDto { Id = Guid.NewGuid(), Title = request.Title ?? string.Empty, OccurredAt = request.OccurredAt, Category = request.Category, Urgency = request.Urgency };
            }

            public IEnumerable<LiveEventDto> GetLiveEvents()
            {
                _onCall();
                return Enumerable.Empty<LiveEventDto>();
            }

            public IEnumerable<HistoricalEventDto> GetHistoricalEvents()
            {
                _onCall();
                return new[] { new HistoricalEventDto { Id = Guid.NewGuid(), Title = "x", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Unknown } };
            }
        }
    }
}
