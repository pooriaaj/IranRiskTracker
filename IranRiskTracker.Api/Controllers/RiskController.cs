using Microsoft.AspNetCore.Mvc;
using IranRiskTracker.Application.Interfaces;
using System.Threading.Tasks;

namespace IranRiskTracker.Api.Controllers
{
    [ApiController]
    [Route("api/risk")]
    public class RiskController : ControllerBase
    {
        private readonly IRiskCalculator _riskCalculator;

        public RiskController(IRiskCalculator riskCalculator)
        {
            _riskCalculator = riskCalculator;
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrent()
        {
            var result = await _riskCalculator.GetCurrentRiskAsync();
            return Ok(result);
        }
    }
}
