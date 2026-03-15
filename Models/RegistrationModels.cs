using System;

namespace WebApplication1.Models
{
    /// <summary>
    /// Step 1: User Details
    /// </summary>
    public class UserDetailsDto
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string MobileNumber { get; set; }
    }

    public class PersonalBankInfoDto
    {
        public required string FatherName { get; set; }
        public required string MotherName { get; set; }
        public required string AadhaarNumber { get; set; }
        public required string AccountNumber { get; set; }
        public required string CifNumber { get; set; }
    }

    public class ProfileSetupDto
    {
        public required string ProfilePassword { get; set; }
        public required DateTime DateOfBirth { get; set; }
    }

    public class CardVerificationDto
    {
        public required string CardNumber { get; set; }
        public required string ExpiryDate { get; set; }
        public required string Cvv { get; set; }
        public required string AtmPin { get; set; }
    }

    public class OtpVerificationDto
    {
        public required string Otp { get; set; }
    }

    public class UserRegistrationRequestDto
    {
        public required UserDetailsDto UserDetails { get; set; }
        public required PersonalBankInfoDto PersonalBankInfo { get; set; }
        public required ProfileSetupDto ProfileSetup { get; set; }
        public required CardVerificationDto CardVerification { get; set; }
        public required OtpVerificationDto OtpVerification { get; set; }
    }

    public class UserRegistrationResponseDto
    {
        public required string UserId { get; set; }
        public required string Username { get; set; }
        public required string Message { get; set; }
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
    }

    public class StepVerificationResponseDto
    {
        public int CurrentStep { get; set; }
        public required string Status { get; set; }
        public required string Message { get; set; }
        public bool CanProceed { get; set; }
    }

    public class RegistrationDetailsDto
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public string? MobileNumber { get; set; }
        public string? FatherName { get; set; }
        public string? MotherName { get; set; }
        public string? AadhaarNumber { get; set; }
        public string? AccountNumber { get; set; }
        public string? CifNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? CardLast4 { get; set; }
        public string? CardExpiry { get; set; }
        public string? MaskedCard { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}