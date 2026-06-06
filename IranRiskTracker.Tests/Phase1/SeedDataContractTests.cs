using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;
using IranRiskTracker.Infrastructure.Seeding;
using IranRiskTracker.Domain.Entities;

namespace IranRiskTracker.Tests.Phase1
{
    /// <summary>
    /// Validates the contract of JSON seed data used in Phase 1 to ensure the application
    /// services receive well-formed deterministic inputs.
    /// </summary>
    public class SeedDataContractTests
    {
        private static string FindSeedDataPath()
        {
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
        public void HistoricalEvents_ShouldMeetContract()
        {
            // Arrange
            var basePath = FindSeedDataPath();
            var provider = new JsonSeedDataProvider(basePath);

            // Act
            var events = provider.GetHistoricalEvents().OrderBy(e => e.OccurredAt).ThenBy(e => e.Title).ToList();

            // Assert
            events.Should().NotBeNullOrEmpty();
            events.Select(e => e.Id).Should().NotContain(Guid.Empty);
            events.Should().OnlyHaveUniqueItems(e => e.Id);
            events.Should().OnlyHaveUniqueItems(e => e.Title);

            foreach (var ev in events)
            {
                ev.Title.Should().NotBeNullOrWhiteSpace();
                ev.Description.Should().NotBeNullOrWhiteSpace();
                ev.OccurredAt.Should().NotBe(default(DateTime));
                if (ev.VerifiedAt.HasValue)
                {
                    ev.VerifiedAt.Value.Should().BeOnOrAfter(ev.OccurredAt);
                }
                Enum.IsDefined(typeof(Domain.Enums.EventCategory), ev.Category).Should().BeTrue();
            }

            // deterministic ordering check: repeated ordering yields same sequence
            var seq1 = events.Select(e => e.Id).ToArray();
            var seq2 = provider.GetHistoricalEvents().OrderBy(e => e.OccurredAt).ThenBy(e => e.Title).Select(e => e.Id).ToArray();
            seq1.Should().Equal(seq2);
        }

        [Fact]
        public void Indicators_ShouldMeetContract()
        {
            // Arrange
            var basePath = FindSeedDataPath();
            var provider = new JsonSeedDataProvider(basePath);

            // Act
            var indicators = provider.GetIndicators().ToList();

            // Assert
            indicators.Should().NotBeNullOrEmpty();
            indicators.Select(i => i.Id).Should().NotContain(Guid.Empty);
            indicators.Should().OnlyHaveUniqueItems(i => i.Id);
            indicators.Select(i => i.Key).Should().OnlyHaveUniqueItems();

            foreach (var ind in indicators)
            {
                ind.Name.Should().NotBeNullOrWhiteSpace();
                ind.Key.Should().NotBeNullOrWhiteSpace();
                Enum.IsDefined(typeof(Domain.Enums.EventCategory), ind.Category).Should().BeTrue();
                ind.Weight.Should().BeGreaterThan(0m).And.BeLessOrEqualTo(1m);
                (ind.DirectionMultiplier == 1 || ind.DirectionMultiplier == -1).Should().BeTrue();
            }

            // total weight must equal 1.0m
            var total = indicators.Sum(i => i.Weight);
            total.Should().Be(1.0m);
        }

        [Fact]
        public void Sources_ShouldMeetContract()
        {
            // Arrange
            var basePath = FindSeedDataPath();
            var provider = new JsonSeedDataProvider(basePath);

            // Act
            var sources = provider.GetSources().ToList();

            // Assert
            sources.Should().NotBeNull();
            sources.Select(s => s.Id).Should().NotContain(Guid.Empty);
            sources.Should().OnlyHaveUniqueItems(s => s.Id);

            foreach (var s in sources)
            {
                s.Name.Should().NotBeNullOrWhiteSpace();
                s.Url.Should().NotBeNullOrWhiteSpace();
                s.Credibility.Value.Should().BeGreaterOrEqualTo(0m).And.BeLessOrEqualTo(1m);
                Enum.IsDefined(typeof(Domain.Enums.SourceBias), s.Bias).Should().BeTrue();
            }
        }
    }
}
