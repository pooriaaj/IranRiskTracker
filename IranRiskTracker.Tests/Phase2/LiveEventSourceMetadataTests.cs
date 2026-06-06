using System;
using FluentAssertions;
using Xunit;
using IranRiskTracker.Infrastructure.Seeding;
using IranRiskTracker.Application.Services;
using IranRiskTracker.Infrastructure.Storage;
using IranRiskTracker.Application.DTOs;

namespace IranRiskTracker.Tests.Phase2
{
    public class LiveEventSourceMetadataTests
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
        public void ValidRequest_StoresSourceMetadata()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var svc = new EventQueryService(seed, liveStore);

            var req = new LiveEventCreateRequest { Title = "t", RawContent = "c", SourceName = "source", SourceUrl = "https://x", SourceHandle = "@h", OwnerNotes = "n", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Protests, Urgency = Domain.Enums.UrgencyLevel.Low };

            var added = svc.AcceptLiveEvent(req);
            added.SourceName.Should().Be("source");
            added.SourceUrl.Should().Be("https://x");
            added.SourceHandle.Should().Be("@h");
            added.OwnerNotes.Should().Be("n");
        }

        [Fact]
        public void MissingSourceName_IsRejected()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var svc = new EventQueryService(seed, liveStore);

            var req = new LiveEventCreateRequest { Title = "t", RawContent = "c", SourceName = "   ", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Protests, Urgency = Domain.Enums.UrgencyLevel.Low };

            Action act = () => svc.AcceptLiveEvent(req);
            act.Should().Throw<ArgumentException>().WithMessage("*sourceName is required*");
        }

        [Fact]
        public void TooLongOwnerNotes_IsRejected()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var svc = new EventQueryService(seed, liveStore);

            var longNotes = new string('x', 2001);
            var req = new LiveEventCreateRequest { Title = "t", RawContent = "c", OwnerNotes = longNotes, SourceName = "s", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Protests, Urgency = Domain.Enums.UrgencyLevel.Low };

            Action act = () => svc.AcceptLiveEvent(req);
            act.Should().Throw<ArgumentException>().WithMessage("*ownerNotes must be at most 2000 characters*");
        }

        [Fact]
        public void Controller_ReturnsBadRequest_ForMissingSourceName()
        {
            var fake = new FakeEventQueryServiceForController();
            var controller = new IranRiskTracker.Api.Controllers.EventsController(fake);

            var res = controller.PostLive(new LiveEventCreateRequest { Title = "t", RawContent = "c", SourceName = "   ", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Protests, Urgency = Domain.Enums.UrgencyLevel.Low });

            res.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
        }

        private class FakeEventQueryServiceForController : IranRiskTracker.Application.Interfaces.IEventQueryService
        {
            public LiveEventDto AcceptLiveEvent(LiveEventCreateRequest request)
            {
                var basePath = FindSeedDataPath();
                var seed = new JsonSeedDataProvider(basePath);
                var liveStore = new InMemoryLiveEventStore();
                var real = new EventQueryService(seed, liveStore);
                return real.AcceptLiveEvent(request);
            }

            public System.Collections.Generic.IEnumerable<LiveEventDto> GetLiveEvents()
            {
                return System.Linq.Enumerable.Empty<LiveEventDto>();
            }

            public System.Collections.Generic.IEnumerable<HistoricalEventDto> GetHistoricalEvents()
            {
                return new[] { new HistoricalEventDto { Id = Guid.NewGuid(), Title = "x", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Unknown } };
            }
        }
    }
}
