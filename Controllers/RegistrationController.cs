using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// API for multi-step user registration (5 steps).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly ILogger<RegistrationController> _logger;
        private readonly IRegistrationRepository _repo;

        public RegistrationController(ILogger<RegistrationController> logger, IRegistrationRepository repo)
        {
            _logger = logger;
            _repo = repo;
        }

        [HttpPost("step1/user-details")]
        public async Task<IActionResult> RegisterUserDetails([FromBody] UserDetailsDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _logger.LogInformation("Step 1: received user details for {Username}", request.Username);

            await _repo.SaveUserDetailsAsync(request);

            var response = new StepVerificationResponseDto
            {
                CurrentStep = 1,
                Status = "Success",
                Message = "User details accepted",
                CanProceed = true
            };

            return Ok(response);
        }

        [HttpPost("step2/personal-bank-info")]
        public async Task<IActionResult> VerifyPersonalBankInfo(
            [FromHeader(Name = "X-Username")] string? username,
            [FromBody] PersonalBankInfoDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (string.IsNullOrWhiteSpace(username)) return BadRequest("Missing X-Username header");

            _logger.LogInformation("Step 2: personal/bank info received for {Aadhaar}", request.AadhaarNumber);

            await _repo.SavePersonalBankInfoAsync(username, request);

            var response = new StepVerificationResponseDto
            {
                CurrentStep = 2,
                Status = "Success",
                Message = "Personal & bank info verified",
                CanProceed = true
            };

            return Ok(response);
        }

        [HttpPost("step3/profile-setup")]
        public async Task<IActionResult> SetupProfile(
            [FromHeader(Name = "X-Username")] string? username,
            [FromBody] ProfileSetupDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (string.IsNullOrWhiteSpace(username)) return BadRequest("Missing X-Username header");

            await _repo.SaveProfileSetupAsync(username, request);

            var response = new StepVerificationResponseDto
            {
                CurrentStep = 3,
                Status = "Success",
                Message = "Profile setup completed",
                CanProceed = true
            };

            return Ok(response);
        }

        [HttpPost("step4/card-verification")]
        public async Task<IActionResult> VerifyCard(
            [FromHeader(Name = "X-Username")] string? username,
            [FromBody] CardVerificationDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (string.IsNullOrWhiteSpace(username)) return BadRequest("Missing X-Username header");

            _logger.LogInformation("Step 4: card verification for card ending {Last4}", request.CardNumber?.Length >= 4 ? request.CardNumber[^4..] : "N/A");

            await _repo.SaveCardVerificationAsync(username, request);

            var response = new StepVerificationResponseDto
            {
                CurrentStep = 4,
                Status = "Success",
                Message = "Card verified; OTP sent to registered mobile",
                CanProceed = true
            };

            return Ok(response);
        }

        [HttpPost("step5/otp-verification")]
        public async Task<IActionResult> VerifyOtp(
            [FromHeader(Name = "X-Username")] string? username,
            [FromBody] OtpVerificationDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (string.IsNullOrWhiteSpace(username)) return BadRequest("Missing X-Username header");

            await _repo.SaveOtpVerificationAsync(username, request);

            var existing = await _repo.GetRegistrationAsync(username);

            var response = new UserRegistrationResponseDto
            {
                UserId = existing?.UserId ?? username,
                Username = username,
                Message = "Registration completed",
                RegistrationDate = DateTime.UtcNow
            };

            return Ok(response);
        }

        [HttpPost("complete")]
        public async Task<IActionResult> CompleteRegistration([FromBody] UserRegistrationRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _logger.LogInformation("Complete registration payload received for {Username}", request.UserDetails.Username);

            await _repo.CompleteRegistrationAsync(request);

            var response = new UserRegistrationResponseDto
            {
                UserId = request.UserDetails.Username,
                Username = request.UserDetails.Username,
                Message = "Registration completed (single call)",
                RegistrationDate = DateTime.UtcNow
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRegistrationById(int id)
        {
            var registration = await _repo.GetRegistrationByIdAsync(id);
            if (registration == null) return NotFound(new { Message = "Registration not found" });
            return Ok(registration);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRegistrations()
        {
            var registrations = await _repo.GetAllRegistrationsAsync();
            return Ok(registrations);
        }
    }
}