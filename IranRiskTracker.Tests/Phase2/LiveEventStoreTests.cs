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
    public class LiveEventStoreTests
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
        public void AcceptLiveEvent_StoresEvent()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var sut = new EventQueryService(seed, liveStore);

            var req = new LiveEventCreateRequest { Title = "t1", RawContent = "r", SourceName = "s", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Protests, Urgency = Domain.Enums.UrgencyLevel.Low };

            var added = sut.AcceptLiveEvent(req);

            added.Should().NotBeNull();
            var all = sut.GetLiveEvents().ToList();
            all.Should().ContainSingle().Which.Id.Should().Be(added.Id);
        }

        [Fact]
        public void GetLiveEvents_ReturnsSubmittedEvents_NewestFirst()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var sut = new EventQueryService(seed, liveStore);

            var a = sut.AcceptLiveEvent(new LiveEventCreateRequest { Title = "a", RawContent = "r", SourceName = "s", OccurredAt = DateTime.UtcNow.AddMinutes(-5), Category = Domain.Enums.EventCategory.Protests, Urgency = Domain.Enums.UrgencyLevel.Low });
            System.Threading.Thread.Sleep(5);
            var b = sut.AcceptLiveEvent(new LiveEventCreateRequest { Title = "b", RawContent = "r", SourceName = "s", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Protests, Urgency = Domain.Enums.UrgencyLevel.Low });

            var all = sut.GetLiveEvents().ToList();
            all.Count.Should().Be(2);
            all.First().Id.Should().Be(b.Id);
            all.Last().Id.Should().Be(a.Id);
        }

        [Fact]
        public void EventsController_DelegatesToService_ForLiveEndpoints()
        {
            var calledGet = false;
            var calledPost = false;
            var fake = new FakeEventQueryService(() => calledGet = true, () => calledPost = true);
            var controller = new IranRiskTracker.Api.Controllers.EventsController(fake);

            var resGet = controller.GetLive();
            calledGet.Should().BeTrue();

            var resPost = controller.PostLive(new LiveEventCreateRequest { Title = "x", RawContent = "r", SourceName = "s", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Protests, Urgency = Domain.Enums.UrgencyLevel.Low });
            calledPost.Should().BeTrue();
        }

        private class FakeEventQueryService : IranRiskTracker.Application.Interfaces.IEventQueryService
        {
            private readonly Action _onGet;
            private readonly Action _onPost;

            public FakeEventQueryService(Action onGet, Action onPost)
            {
                _onGet = onGet;
                _onPost = onPost;
            }

            public LiveEventDto AcceptLiveEvent(LiveEventCreateRequest request)
            {
                _onPost();
                return new LiveEventDto { Id = Guid.NewGuid(), Title = request.Title ?? string.Empty, OccurredAt = request.OccurredAt, Category = request.Category, Urgency = request.Urgency };
            }

            public System.Collections.Generic.IEnumerable<LiveEventDto> GetLiveEvents()
            {
                _onGet();
                return System.Linq.Enumerable.Empty<LiveEventDto>();
            }

            public System.Collections.Generic.IEnumerable<HistoricalEventDto> GetHistoricalEvents()
            {
                return new[] { new HistoricalEventDto { Id = Guid.NewGuid(), Title = "x", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Unknown } };
            }
        }
    }
}
