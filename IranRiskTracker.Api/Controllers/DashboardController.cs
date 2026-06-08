using Microsoft.AspNetCore.Mvc;
using IranRiskTracker.Application.Interfaces;

namespace IranRiskTracker.Api.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardSummaryService _svc;

        public DashboardController(IDashboardSummaryService svc)
        {
            _svc = svc;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await _svc.GetSummaryAsync();
            return Ok(summary);
        }
    }
}
