using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Repositories;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SmsController : ControllerBase
    {
        private readonly ISmsService _service;
        private readonly IRegistrationRepository _repo;
        private readonly ILogger<SmsController> _logger;

        public SmsController(ISmsService service, IRegistrationRepository repo, ILogger<SmsController> logger)
        {
            _service = service;
            _repo = repo;
            _logger = logger;
        }

        /// <summary>Send SMS to a single number</summary>
        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SmsDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _logger.LogInformation("Sending SMS to {To}", dto.To);
            try { return Ok(await _service.SendAsync(dto)); }
            catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
        }

        /// <summary>Send SMS to multiple numbers</summary>
        [HttpPost("send-bulk")]
        public async Task<IActionResult> SendBulk([FromBody] SmsBulkDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _logger.LogInformation("Sending bulk SMS to {Count} numbers", dto.Numbers.Count);
            try { return Ok(await _service.SendBulkAsync(dto)); }
            catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
        }

        /// <summary>Send SMS to all registered users</summary>
        [HttpPost("send-all")]
        public async Task<IActionResult> SendAll([FromBody] SmsAllDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var users = await _repo.GetAllRegistrationsAsync();
            var numbers = users
                .Where(u => !string.IsNullOrWhiteSpace(u.MobileNumber))
                .Select(u => u.MobileNumber!)
                .Distinct()
                .ToList();
            if (numbers.Count == 0) return BadRequest(new { error = "No registered users with mobile numbers found" });
            _logger.LogInformation("Sending SMS to all {Count} registered users", numbers.Count);
            try { return Ok(await _service.SendBulkAsync(new SmsBulkDto { Numbers = numbers, Message = dto.Message })); }
            catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
        }
    }
}
