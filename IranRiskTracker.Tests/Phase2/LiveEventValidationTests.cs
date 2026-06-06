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
    public class LiveEventValidationTests
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
        public void ValidRequest_IsStored()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var sut = new EventQueryService(seed, liveStore);

            var req = new LiveEventCreateRequest { Title = "ok", RawContent = "content", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Protests, Urgency = Domain.Enums.UrgencyLevel.Low };

            var added = sut.AcceptLiveEvent(req);
            added.Should().NotBeNull();
        }

        [Fact]
        public void EmptyTitle_ThrowsArgumentException()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var sut = new EventQueryService(seed, liveStore);

            var req = new LiveEventCreateRequest { Title = "   ", RawContent = "content", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Protests, Urgency = Domain.Enums.UrgencyLevel.Low };

            Action act = () => sut.AcceptLiveEvent(req);
            act.Should().Throw<ArgumentException>().WithMessage("*title is required*");
        }

        [Fact]
        public void EmptyRawContent_ThrowsArgumentException()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var sut = new EventQueryService(seed, liveStore);

            var req = new LiveEventCreateRequest { Title = "t", RawContent = "   ", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Protests, Urgency = Domain.Enums.UrgencyLevel.Low };

            Action act = () => sut.AcceptLiveEvent(req);
            act.Should().Throw<ArgumentException>().WithMessage("*rawContent is required*");
        }

        [Fact]
        public void UnknownCategory_IsRejected()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var sut = new EventQueryService(seed, liveStore);

            var req = new LiveEventCreateRequest { Title = "t", RawContent = "c", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Unknown, Urgency = Domain.Enums.UrgencyLevel.Low };

            Action act = () => sut.AcceptLiveEvent(req);
            act.Should().Throw<ArgumentException>().WithMessage("*category must be a defined non-Unknown EventCategory*");
        }

        [Fact]
        public void OccurredAtTooFarInFuture_IsRejected()
        {
            var basePath = FindSeedDataPath();
            var seed = new JsonSeedDataProvider(basePath);
            var liveStore = new InMemoryLiveEventStore();
            var sut = new EventQueryService(seed, liveStore);

            var req = new LiveEventCreateRequest { Title = "t", RawContent = "c", OccurredAt = DateTime.UtcNow.AddDays(2), Category = Domain.Enums.EventCategory.Protests, Urgency = Domain.Enums.UrgencyLevel.Low };

            Action act = () => sut.AcceptLiveEvent(req);
            act.Should().Throw<ArgumentException>().WithMessage("*occurredAt cannot be more than 1 day in the future*");
        }

        [Fact]
        public void Controller_ReturnsBadRequest_ForInvalid()
        {
            var fake = new FakeEventQueryServiceForController();
            var controller = new IranRiskTracker.Api.Controllers.EventsController(fake);

            var res = controller.PostLive(new LiveEventCreateRequest { Title = "   ", RawContent = "c", OccurredAt = DateTime.UtcNow, Category = Domain.Enums.EventCategory.Protests, Urgency = Domain.Enums.UrgencyLevel.Low });

            res.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
        }

        private class FakeEventQueryServiceForController : IranRiskTracker.Application.Interfaces.IEventQueryService
        {
            public LiveEventDto AcceptLiveEvent(LiveEventCreateRequest request)
            {
                // delegate to real validation to simulate service behavior
                var basePath = FindSeedDataPath();
                var seed = new JsonSeedDataProvider(basePath);
                var liveStore = new InMemoryLiveEventStore();
                var real = new EventQueryService(seed, liveStore);
                try
                {
                    return real.AcceptLiveEvent(request);
                }
                catch (ArgumentException ex)
                {
                    throw;
                }
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
