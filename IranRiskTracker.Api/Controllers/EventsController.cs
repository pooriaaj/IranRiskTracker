using Microsoft.AspNetCore.Mvc;
using IranRiskTracker.Infrastructure.Seed;
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
            var path = Path.Combine(AppContext.BaseDirectory, "Seed/data/historicalEvents.json");
            var events = SeedLoader.LoadHistoricalEvents(path)
                .Select(e => new HistoricalEventDto
                {
                    Id = e.Id,
                    OccurredAt = e.OccurredAt,
                    Title = e.Title,
                    Details = e.Details,
                    Severity = e.Severity
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
    }
}
