using Microsoft.AspNetCore.Mvc;
using IranRiskTracker.Application.Interfaces;

namespace IranRiskTracker.Api.Controllers
{
    [ApiController]
    [Route("api/snapshots")]
    public class SnapshotsController : ControllerBase
    {
        private readonly IRiskCalculator _riskCalculator;
        private readonly IranRiskTracker.Application.Interfaces.IRiskSnapshotStore? _snapshotStore;

        public SnapshotsController(IRiskCalculator riskCalculator, IranRiskTracker.Application.Interfaces.IRiskSnapshotStore? snapshotStore = null)
        {
            _riskCalculator = riskCalculator;
            _snapshotStore = snapshotStore;
        }

        /// <summary>
        /// Returns the current seed-backed risk snapshot.
        /// </summary>
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest()
        {
            var latest = _snapshotStore?.GetLatest();
            if (latest != null) return Ok(latest);

            var result = await _riskCalculator.GetCurrentRiskAsync();
            return Ok(result);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var all = _snapshotStore?.GetAll() ?? Array.Empty<IranRiskTracker.Application.DTOs.RiskDto>();
            return Ok(all);
        }
    }
}
