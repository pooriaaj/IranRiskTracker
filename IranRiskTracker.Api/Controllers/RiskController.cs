using Microsoft.AspNetCore.Mvc;
using IranRiskTracker.Application.Interfaces;
using System.Threading.Tasks;

namespace IranRiskTracker.Api.Controllers
{
    /// <summary>
    /// Exposes current risk snapshots calculated by the application layer.
    /// </summary>
    [ApiController]
    [Route("api/risk")]
    public class RiskController : ControllerBase
    {
        private readonly IRiskCalculator _riskCalculator;

        public RiskController(IRiskCalculator riskCalculator)
        {
            _riskCalculator = riskCalculator;
        }

        /// <summary>
        /// Returns the current seed-backed baseline risk score.
        /// </summary>
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrent()
        {
            var result = await _riskCalculator.GetCurrentRiskAsync();
            return Ok(result);
        }
    }
}
