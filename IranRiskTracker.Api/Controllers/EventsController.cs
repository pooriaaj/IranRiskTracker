using Microsoft.AspNetCore.Mvc;
using IranRiskTracker.Infrastructure.Seeding;
using IranRiskTracker.Application.DTOs;
using System.Linq;

namespace IranRiskTracker.Api.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public EventsController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("historical")]
        public IActionResult GetHistorical()
        {
            var events = JsonSeeder.LoadHistoricalEvents(Path.Combine(AppContext.BaseDirectory, "IranRiskTracker.Infrastructure", "Seeding", "Data"))
                .Select(e => new HistoricalEventDto
                {
                    Id = e.Id,
                    OccurredAt = e.OccurredAt,
                    Title = e.Title,
                    Details = e.Description,
                    Severity = 0
                })
                .ToList();

            return Ok(events);
        }

        [HttpGet("live")]
        public IActionResult GetLive()
        {
            // Phase 1: No live ingest yet. Return an empty collection placeholder.
            return Ok(Enumerable.Empty<object>());
        }

        [HttpPost("live")]
        public IActionResult PostLive([FromBody] LiveEventCreateRequest request)
        {
            // Phase 1: accept minimal live event payload and return a stubbed LiveEvent with generated Id.
            var live = new IranRiskTracker.Domain.Entities.LiveEvent
            {
                Id = Guid.NewGuid(),
                Title = request.Title ?? string.Empty,
                RawContent = request.RawContent,
                OccurredAt = request.OccurredAt,
                IngestedAt = DateTime.UtcNow,
                Category = request.Category,
                Urgency = request.Urgency,
                IsProcessed = false
            };

            // In Phase 1 we do not persist; return 201 Created with resource location header.
            return CreatedAtAction(nameof(GetLive), new { id = live.Id }, new { id = live.Id, status = "ingested" });
        }
    }

    public record LiveEventCreateRequest(string? Title, string? RawContent, DateTime OccurredAt, int Urgency, IranRiskTracker.Domain.Enums.EventCategory Category);
}
