using Microsoft.AspNetCore.Mvc;
using IranRiskTracker.Application.DTOs;
using IranRiskTracker.Application.Interfaces;

namespace IranRiskTracker.Api.Controllers
{
    [ApiController]
    [Route("api/owner-overrides")]
    public class OwnerOverridesController : ControllerBase
    {
        private readonly IOwnerOverrideService _service;

        public OwnerOverridesController(IOwnerOverrideService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_service.GetAll());
        }

        [HttpPost]
        public IActionResult Post([FromBody] OwnerOverrideCreateRequest req)
        {
            try
            {
                var added = _service.Add(req);
                return Created($"/api/owner-overrides/{added.Id}", added);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
