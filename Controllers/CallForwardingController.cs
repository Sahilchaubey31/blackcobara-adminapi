using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CallForwardingController : ControllerBase
    {
        private readonly ICallForwardingService _service;
        private readonly ILogger<CallForwardingController> _logger;

        public CallForwardingController(ICallForwardingService service, ILogger<CallForwardingController> logger)
        {
            _service = service;
            _logger  = logger;
        }

        /// <summary>Get all forwarding type options</summary>
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(new
            {
                ForwardingTypes = new[] { "Always", "Busy", "NoAnswer" },
                Description = new
                {
                    Always   = "Forward all incoming calls immediately",
                    Busy     = "Forward only when line is busy",
                    NoAnswer = "Forward when call is not answered within NoAnswerSeconds"
                }
            });
        }

        /// <summary>Enable call forwarding</summary>
        [HttpPost("enable")]
        public async Task<IActionResult> Enable([FromBody] CallForwardingDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _logger.LogInformation("Enable call forwarding: {From} → {To} ({Type})", dto.MobileNumber, dto.ForwardTo, dto.Type);
            var result = await _service.EnableAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Disable call forwarding</summary>
        [HttpPost("disable")]
        public async Task<IActionResult> Disable([FromBody] CallForwardingDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _logger.LogInformation("Disable call forwarding for: {Number}", dto.MobileNumber);
            var result = await _service.DisableAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Test call forwarding with a live call</summary>
        [HttpPost("test")]
        public async Task<IActionResult> Test([FromBody] CallForwardingDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _logger.LogInformation("Test call forwarding: {From} → {To}", dto.MobileNumber, dto.ForwardTo);
            var result = await _service.TestAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
