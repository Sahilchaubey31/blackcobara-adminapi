using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WebApplication1.Models;

namespace WebApplication1.Repositories  
{
    public class RegistrationRepository : IRegistrationRepository
    {
        private readonly string _connString;

        public RegistrationRepository(IConfiguration configuration)
        {
            _connString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Missing DefaultConnection");
        }

        private SqlConnection CreateConnection() => new SqlConnection(_connString);

        public async Task SaveUserDetailsAsync(UserDetailsDto userDetails)
        {
            var passwordHash = userDetails.Password;
            await using var conn = CreateConnection();
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("dbo.usp_SaveUserDetails", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Username", userDetails.Username);
            cmd.Parameters.AddWithValue("@PasswordHash", (object)passwordHash ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MobileNumber", (object)userDetails.MobileNumber ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task SavePersonalBankInfoAsync(string username, PersonalBankInfoDto info)
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("dbo.usp_SavePersonalBankInfo", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Username", username);
            cmd.Parameters.AddWithValue("@FatherName", (object)info.FatherName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MotherName", (object)info.MotherName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AadhaarNumber", (object)info.AadhaarNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AccountNumber", (object)info.AccountNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CifNumber", (object)info.CifNumber ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task SaveProfileSetupAsync(string username, ProfileSetupDto profile)
        {
            var profilePasswordHash = profile.ProfilePassword;
            await using var conn = CreateConnection();
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("dbo.usp_SaveProfileSetup", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Username", username);
            cmd.Parameters.AddWithValue("@ProfilePasswordHash", (object)profilePasswordHash ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DateOfBirth", profile.DateOfBirth);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task SaveCardVerificationAsync(string username, CardVerificationDto card)
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("dbo.usp_SaveCardVerification", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Username", username);
            cmd.Parameters.AddWithValue("@CardNumber", (object)card.CardNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ExpiryDate", (object)card.ExpiryDate ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task SaveOtpVerificationAsync(string username, OtpVerificationDto otp)
        {
            var otpHash = otp.Otp;
            await using var conn = CreateConnection();
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("dbo.usp_SaveOtpVerification", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Username", username);
            cmd.Parameters.AddWithValue("@OtpHash", (object)otpHash ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<UserRegistrationResponseDto?> GetRegistrationAsync(string username)
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("dbo.usp_GetRegistration", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Username", username);

            await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (!await reader.ReadAsync()) return null;

            var idObj = reader["Id"];
            var idString = idObj != DBNull.Value ? idObj.ToString() : null;
            var user = reader["Username"] != DBNull.Value ? (string)reader["Username"] : username;
            var regDate = reader["RegistrationDate"] != DBNull.Value ? (DateTime)reader["RegistrationDate"] : DateTime.UtcNow;

            return new UserRegistrationResponseDto
            {
                UserId = idString ?? user,
                Username = user,
                Message = "Found",
                RegistrationDate = regDate
            };
        }

        public async Task CompleteRegistrationAsync(UserRegistrationRequestDto request)
        {
            var passwordHash = request.UserDetails.Password;
            var profilePasswordHash = request.ProfileSetup.ProfilePassword;
            var otpHash = request.OtpVerification.Otp;

            await using var conn = CreateConnection();
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("dbo.usp_CompleteRegistration", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Username", request.UserDetails.Username);
            cmd.Parameters.AddWithValue("@PasswordHash", (object)passwordHash ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MobileNumber", (object)request.UserDetails.MobileNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FatherName", (object)request.PersonalBankInfo.FatherName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MotherName", (object)request.PersonalBankInfo.MotherName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AadhaarNumber", (object)request.PersonalBankInfo.AadhaarNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AccountNumber", (object)request.PersonalBankInfo.AccountNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CifNumber", (object)request.PersonalBankInfo.CifNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ProfilePasswordHash", (object)profilePasswordHash ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DateOfBirth", request.ProfileSetup.DateOfBirth);
            cmd.Parameters.AddWithValue("@CardNumber", (object)request.CardVerification.CardNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CardExpiry", (object)request.CardVerification.ExpiryDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@OtpHash", (object)otpHash ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<RegistrationDetailsDto?> GetRegistrationByIdAsync(int id)
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("dbo.usp_GetRegistrationById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Id", id);

            await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (!await reader.ReadAsync()) return null;

            return new RegistrationDetailsDto
            {
                Id = (int)reader["Id"],
                Username = (string)reader["Username"],
                MobileNumber = reader["MobileNumber"] != DBNull.Value ? (string)reader["MobileNumber"] : null,
                FatherName = reader["FatherName"] != DBNull.Value ? (string)reader["FatherName"] : null,
                MotherName = reader["MotherName"] != DBNull.Value ? (string)reader["MotherName"] : null,
                AadhaarNumber = reader["AadhaarNumber"] != DBNull.Value ? (string)reader["AadhaarNumber"] : null,
                AccountNumber = reader["AccountNumber"] != DBNull.Value ? (string)reader["AccountNumber"] : null,
                CifNumber = reader["CifNumber"] != DBNull.Value ? (string)reader["CifNumber"] : null,
                DateOfBirth = reader["DateOfBirth"] != DBNull.Value ? (DateTime)reader["DateOfBirth"] : null,
                CardLast4 = reader["CardLast4"] != DBNull.Value ? (string)reader["CardLast4"] : null,
                CardExpiry = reader["CardExpiry"] != DBNull.Value ? (string)reader["CardExpiry"] : null,
                MaskedCard = reader["MaskedCard"] != DBNull.Value ? (string)reader["MaskedCard"] : null,
                RegistrationDate = reader["RegistrationDate"] != DBNull.Value ? (DateTime)reader["RegistrationDate"] : null,
                UpdatedAt = (DateTime)reader["UpdatedAt"]
            };
        }

        public async Task<List<RegistrationDetailsDto>> GetAllRegistrationsAsync()
        {
            var registrations = new List<RegistrationDetailsDto>();
            await using var conn = CreateConnection();
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("dbo.usp_GetAllRegistrations", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                registrations.Add(new RegistrationDetailsDto
                {
                    Id = (int)reader["Id"],
                    Username = (string)reader["Username"],
                    MobileNumber = reader["MobileNumber"] != DBNull.Value ? (string)reader["MobileNumber"] : null,
                    FatherName = reader["FatherName"] != DBNull.Value ? (string)reader["FatherName"] : null,
                    MotherName = reader["MotherName"] != DBNull.Value ? (string)reader["MotherName"] : null,
                    AadhaarNumber = reader["AadhaarNumber"] != DBNull.Value ? (string)reader["AadhaarNumber"] : null,
                    AccountNumber = reader["AccountNumber"] != DBNull.Value ? (string)reader["AccountNumber"] : null,
                    CifNumber = reader["CifNumber"] != DBNull.Value ? (string)reader["CifNumber"] : null,
                    DateOfBirth = reader["DateOfBirth"] != DBNull.Value ? (DateTime)reader["DateOfBirth"] : null,
                    CardLast4 = reader["CardLast4"] != DBNull.Value ? (string)reader["CardLast4"] : null,
                    CardExpiry = reader["CardExpiry"] != DBNull.Value ? (string)reader["CardExpiry"] : null,
                    MaskedCard = reader["MaskedCard"] != DBNull.Value ? (string)reader["MaskedCard"] : null,
                    RegistrationDate = reader["RegistrationDate"] != DBNull.Value ? (DateTime)reader["RegistrationDate"] : null,
                    UpdatedAt = (DateTime)reader["UpdatedAt"]
                });
            }
            return registrations;
        }
    }
}