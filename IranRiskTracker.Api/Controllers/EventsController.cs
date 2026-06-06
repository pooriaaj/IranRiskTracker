using Microsoft.AspNetCore.Mvc;
using IranRiskTracker.Infrastructure.Seeding;
using IranRiskTracker.Application.DTOs;
using Microsoft.AspNetCore.Hosting;
using IranRiskTracker.Application.DTOs;
using System.Linq;

namespace IranRiskTracker.Api.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public EventsController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet("historical")]
        public IActionResult GetHistorical()
        {
            // TODO Phase 3: replace with IEventRepository call
            var events = JsonSeeder.LoadHistoricalEvents(Path.Combine(_env.ContentRootPath, "Seeding", "Data"))
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
        public IActionResult PostLive([FromBody] LiveEventCreateRequest dto)
        {
            // Phase 1: accept minimal live event payload and return a stubbed LiveEvent with generated Id.
            var live = new IranRiskTracker.Domain.Entities.LiveEvent
            {
                Id = Guid.NewGuid(),
                Title = dto.Title ?? string.Empty,
                RawContent = dto.RawContent,
                OccurredAt = dto.OccurredAt,
                IngestedAt = DateTime.UtcNow,
                Category = dto.Category,
                Urgency = dto.Urgency,
                IsProcessed = false
            };

            // In Phase 1 we do not persist; return 201 Created with resource location header.
            return CreatedAtAction(nameof(GetLive), new { id = live.Id }, new { id = live.Id, status = "ingested" });
        }
    }

    // removed nested record - using application DTO LiveEventCreateRequest
}
