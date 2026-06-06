using Microsoft.AspNetCore.Mvc;
using IranRiskTracker.Application.DTOs;

namespace IranRiskTracker.Api.Controllers
{
    [ApiController]
    [Route("api/snapshots")]
    public class SnapshotsController : ControllerBase
    {
        public SnapshotsController()
        {
        }

        [HttpGet("latest")]
        public IActionResult GetLatest()
        {
            // Phase 1: Return a placeholder snapshot. Real snapshot generation will be implemented in later phases.
            var dto = new RiskDto
            {
                Timestamp = DateTime.UtcNow,
                Level = IranRiskTracker.Domain.Enums.RiskLevel.Unknown,
                Score = 0.0,
                Summary = "Placeholder snapshot - scoring not implemented"
            };

            return Ok(dto);
        }
    }
}
