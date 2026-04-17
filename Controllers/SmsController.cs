using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SmsController : ControllerBase
    {
        private readonly ISmsService _service;
        private readonly ILogger<SmsController> _logger;

        public SmsController(ISmsService service, ILogger<SmsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>Send SMS to a single number</summary>
        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SmsDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _logger.LogInformation("Sending SMS to {To}", dto.To);
            try
            {
                var result = await _service.SendAsync(dto);
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
        }

        /// <summary>Send SMS to multiple numbers</summary>
        [HttpPost("send-bulk")]
        public async Task<IActionResult> SendBulk([FromBody] SmsBulkDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _logger.LogInformation("Sending bulk SMS to {Count} numbers", dto.Numbers.Count);
            try
            {
                var result = await _service.SendBulkAsync(dto);
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
        }
    }
}
