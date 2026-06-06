using Microsoft.AspNetCore.Mvc;
using IranRiskTracker.Application.Interfaces;

namespace IranRiskTracker.Api.Controllers
{
    [ApiController]
    [Route("api/snapshots")]
    public class SnapshotsController : ControllerBase
    {
        private readonly IRiskCalculator _riskCalculator;

        public SnapshotsController(IRiskCalculator riskCalculator)
        {
            _riskCalculator = riskCalculator;
        }

        /// <summary>
        /// Returns the current seed-backed risk snapshot.
        /// </summary>
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest()
        {
            var result = await _riskCalculator.GetCurrentRiskAsync();
            return Ok(result);
        }
    }
}
