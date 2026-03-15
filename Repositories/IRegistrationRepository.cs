using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public interface IRegistrationRepository
    {
        Task SaveUserDetailsAsync(UserDetailsDto userDetails);
        Task SavePersonalBankInfoAsync(string username, PersonalBankInfoDto info);
        Task SaveProfileSetupAsync(string username, ProfileSetupDto profile);
        Task SaveCardVerificationAsync(string username, CardVerificationDto card);
        Task SaveOtpVerificationAsync(string username, OtpVerificationDto otp);
        Task<UserRegistrationResponseDto?> GetRegistrationAsync(string username);
        Task CompleteRegistrationAsync(UserRegistrationRequestDto request);
        Task<RegistrationDetailsDto?> GetRegistrationByIdAsync(int id);
        Task<List<RegistrationDetailsDto>> GetAllRegistrationsAsync();
    }
}