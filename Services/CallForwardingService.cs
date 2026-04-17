using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Twilio.TwiML;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class CallForwardingService : ICallForwardingService
    {
        private readonly IConfiguration _config;

        public CallForwardingService(IConfiguration config)
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

        public async Task<CallForwardingResultDto> EnableAsync(CallForwardingDto dto)
        {
            Init();
            var twiml = BuildForwardTwiml(dto);
            var call = await CallResource.CreateAsync(
                to:   new PhoneNumber(dto.MobileNumber),
                from: new PhoneNumber(FromNumber),
                twiml: new Twilio.Types.Twiml(twiml)
            );
            return new CallForwardingResultDto { Success = true, CallSid = call.Sid, Status = call.Status.ToString(), Message = $"Call forwarding enabled: {dto.MobileNumber} → {dto.ForwardTo} ({dto.Type})" };
        }

        public async Task<CallForwardingResultDto> DisableAsync(CallForwardingDto dto)
        {
            Init();
            var calls = await CallResource.ReadAsync(to: new PhoneNumber(dto.MobileNumber), status: CallResource.StatusEnum.InProgress);
            int cancelled = 0;
            foreach (var call in calls)
            {
                await CallResource.UpdateAsync(pathSid: call.Sid, status: CallResource.UpdateStatusEnum.Canceled);
                cancelled++;
            }
            return new CallForwardingResultDto { Success = true, Message = cancelled > 0 ? $"Call forwarding disabled. {cancelled} active call(s) cancelled for {dto.MobileNumber}" : $"No active forwarded calls found for {dto.MobileNumber}" };
        }

        public async Task<CallForwardingResultDto> TestAsync(CallForwardingDto dto)
        {
            Init();
            var twiml = $"<Response><Say>This is a test call forwarding from {dto.MobileNumber}.</Say><Dial>{dto.ForwardTo}</Dial></Response>";
            var call = await CallResource.CreateAsync(
                to:   new PhoneNumber(dto.MobileNumber),
                from: new PhoneNumber(FromNumber),
                twiml: new Twilio.Types.Twiml(twiml)
            );
            return new CallForwardingResultDto { Success = true, CallSid = call.Sid, Status = call.Status.ToString(), Message = $"Test call initiated from {dto.MobileNumber} to {dto.ForwardTo}" };
        }

        private string BuildForwardTwiml(CallForwardingDto dto)
        {
            return dto.Type switch
            {
                "Always" => $"<Response><Dial>{dto.ForwardTo}</Dial></Response>",

                "Busy" => $@"<Response>
                                <Dial action=""/busy-fallback"">
                                    <Number>{dto.ForwardTo}</Number>
                                </Dial>
                             </Response>",

                "NoAnswer" => $@"<Response>
                                    <Dial timeout=""{dto.NoAnswerSeconds}"">
                                        <Number>{dto.ForwardTo}</Number>
                                    </Dial>
                                 </Response>",

                _ => $"<Response><Dial>{dto.ForwardTo}</Dial></Response>"
            };
        }
    }
}
