using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using IranRiskTracker.Application.Services;
using IranRiskTracker.Infrastructure.Storage;
using IranRiskTracker.Application.DTOs;
using IranRiskTracker.Domain.Enums;

namespace IranRiskTracker.Tests.Phase2
{
    public class OwnerOverrideTests
    {
        [Fact]
        public void ValidOverride_IsStored()
        {
            var store = new InMemoryOwnerOverrideStore();
            var svc = new OwnerOverrideService(store);

            var req = new OwnerOverrideCreateRequest { Title = "t", Reasoning = "r", Category = EventCategory.Cyber, ScoreAdjustment = 5.0, AppliedAt = DateTime.UtcNow };
            var added = svc.Add(req);

            added.Should().NotBeNull();
            store.GetAll().Should().ContainSingle().Which.Id.Should().Be(added.Id);
        }

        [Fact]
        public void InvalidTitle_Rejected()
        {
            var store = new InMemoryOwnerOverrideStore();
            var svc = new OwnerOverrideService(store);

            var req = new OwnerOverrideCreateRequest { Title = "   ", Reasoning = "r", Category = EventCategory.Cyber, ScoreAdjustment = 0.0, AppliedAt = DateTime.UtcNow };
            Action act = () => svc.Add(req);
            act.Should().Throw<ArgumentException>().WithMessage("*title is required*");
        }

        [Fact]
        public void ScoreAboveMax_Rejected()
        {
            var store = new InMemoryOwnerOverrideStore();
            var svc = new OwnerOverrideService(store);

            var req = new OwnerOverrideCreateRequest { Title = "t", Reasoning = "r", Category = EventCategory.Cyber, ScoreAdjustment = 30.0, AppliedAt = DateTime.UtcNow };
            Action act = () => svc.Add(req);
            act.Should().Throw<ArgumentException>().WithMessage("*scoreAdjustment must be between -25 and 25*");
        }

        [Fact]
        public void ScoreBelowMin_Rejected()
        {
            var store = new InMemoryOwnerOverrideStore();
            var svc = new OwnerOverrideService(store);

            var req = new OwnerOverrideCreateRequest { Title = "t", Reasoning = "r", Category = EventCategory.Cyber, ScoreAdjustment = -30.0, AppliedAt = DateTime.UtcNow };
            Action act = () => svc.Add(req);
            act.Should().Throw<ArgumentException>().WithMessage("*scoreAdjustment must be between -25 and 25*");
        }

        [Fact]
        public void UnknownCategory_Rejected()
        {
            var store = new InMemoryOwnerOverrideStore();
            var svc = new OwnerOverrideService(store);

            var req = new OwnerOverrideCreateRequest { Title = "t", Reasoning = "r", Category = EventCategory.Unknown, ScoreAdjustment = 0.0, AppliedAt = DateTime.UtcNow };
            Action act = () => svc.Add(req);
            act.Should().Throw<ArgumentException>().WithMessage("*category must be a defined non-Unknown EventCategory*");
        }

        [Fact]
        public void GetAll_ReturnsNewestFirst()
        {
            var store = new InMemoryOwnerOverrideStore();
            var svc = new OwnerOverrideService(store);

            var a = svc.Add(new OwnerOverrideCreateRequest { Title = "a", Reasoning = "r", Category = EventCategory.Cyber, ScoreAdjustment = 1.0, AppliedAt = DateTime.UtcNow.AddMinutes(-1) });
            var b = svc.Add(new OwnerOverrideCreateRequest { Title = "b", Reasoning = "r", Category = EventCategory.Cyber, ScoreAdjustment = 2.0, AppliedAt = DateTime.UtcNow });

            var all = svc.GetAll().ToList();
            all.First().Id.Should().Be(b.Id);
            all.Last().Id.Should().Be(a.Id);
        }

        [Fact]
        public void Controller_ReturnsBadRequest_ForInvalid()
        {
            var store = new InMemoryOwnerOverrideStore();
            var svc = new OwnerOverrideService(store);
            var controller = new IranRiskTracker.Api.Controllers.OwnerOverridesController(svc);

            var res = controller.Post(new OwnerOverrideCreateRequest { Title = "   ", Reasoning = "r", Category = EventCategory.Cyber, ScoreAdjustment = 0.0, AppliedAt = DateTime.UtcNow });
            res.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
        }
    }
}
