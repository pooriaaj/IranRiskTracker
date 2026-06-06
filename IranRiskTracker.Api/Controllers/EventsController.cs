using Microsoft.AspNetCore.Mvc;
using IranRiskTracker.Application.DTOs;
using IranRiskTracker.Application.Interfaces;

namespace IranRiskTracker.Api.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly IEventQueryService _eventQueryService;

        public EventsController(IEventQueryService eventQueryService)
        {
            _eventQueryService = eventQueryService;
        }

        /// <summary>
        /// Returns historical seed events used to calibrate the Phase 1 baseline.
        /// </summary>
        [HttpGet("historical")]
        public IActionResult GetHistorical()
        {
            return Ok(_eventQueryService.GetHistoricalEvents());
        }

        /// <summary>
        /// Returns live events once a live store is available.
        /// </summary>
        [HttpGet("live")]
        public IActionResult GetLive()
        {
            return Ok(_eventQueryService.GetLiveEvents());
        }

        /// <summary>
        /// Accepts a live event payload and returns the transient event representation.
        /// </summary>
        [HttpPost("live")]
        public IActionResult PostLive([FromBody] LiveEventCreateRequest dto)
        {
            try
            {
                var liveEvent = _eventQueryService.AcceptLiveEvent(dto);
                return Created($"/api/events/live/{liveEvent.Id}", liveEvent);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
