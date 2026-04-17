using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class SmsService : ISmsService
    {
        private readonly IConfiguration _config;

        public SmsService(IConfiguration config)
        {
            _config = config;
        }

        private void Init()
        {
            var sid = _config["Twilio:AccountSid"] ?? throw new InvalidOperationException("Missing Twilio:AccountSid");
            var token = _config["Twilio:AuthToken"] ?? throw new InvalidOperationException("Missing Twilio:AuthToken");
            TwilioClient.Init(sid, token);
        }

        private string FromNumber => _config["Twilio:FromNumber"] ?? throw new InvalidOperationException("Missing Twilio:FromNumber");

        public async Task<SmsResultDto> SendAsync(SmsDto dto)
        {
            Init();
            var msg = await MessageResource.CreateAsync(
                to: new PhoneNumber(dto.To),
                from: new PhoneNumber(FromNumber),
                body: dto.Message
            );
            return new SmsResultDto
            {
                Success = true,
                MessageSid = msg.Sid,
                Status = msg.Status.ToString(),
                Message = $"SMS sent to {dto.To}"
            };
        }

        public async Task<SmsBulkResultDto> SendBulkAsync(SmsBulkDto dto)
        {
            Init();
            var results = new List<SmsResultDto>();
            foreach (var number in dto.Numbers)
            {
                try
                {
                    var msg = await MessageResource.CreateAsync(
                        to: new PhoneNumber(number),
                        from: new PhoneNumber(FromNumber),
                        body: dto.Message
                    );
                    results.Add(new SmsResultDto { Success = true, MessageSid = msg.Sid, Status = msg.Status.ToString(), Message = $"SMS sent to {number}" });
                }
                catch (Exception ex)
                {
                    results.Add(new SmsResultDto { Success = false, Message = $"Failed to send to {number}: {ex.Message}" });
                }
            }
            return new SmsBulkResultDto
            {
                TotalSent = results.Count(r => r.Success),
                TotalFailed = results.Count(r => !r.Success),
                Results = results
            };
        }
    }
}
