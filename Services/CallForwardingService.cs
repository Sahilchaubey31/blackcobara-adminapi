using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Twilio.TwiML;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class CallForwardingService : ICallForwardingService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromNumber;

        public CallForwardingService(IConfiguration config)
        {
            _accountSid = config["Twilio:AccountSid"]!;
            _authToken  = config["Twilio:AuthToken"]!;
            _fromNumber = config["Twilio:FromNumber"]!;
            TwilioClient.Init(_accountSid, _authToken);
        }

        public async Task<CallForwardingResultDto> EnableAsync(CallForwardingDto dto)
        {
            // Build TwiML to forward the call
            var twiml = BuildForwardTwiml(dto);

            var call = await CallResource.CreateAsync(
                to:   new PhoneNumber(dto.MobileNumber),
                from: new PhoneNumber(_fromNumber),
                twiml: new Twilio.Types.Twiml(twiml)
            );

            return new CallForwardingResultDto
            {
                Success   = true,
                CallSid   = call.Sid,
                Status    = call.Status.ToString(),
                Message   = $"Call forwarding enabled: {dto.MobileNumber} → {dto.ForwardTo} ({dto.Type})"
            };
        }

        public async Task<CallForwardingResultDto> DisableAsync(CallForwardingDto dto)
        {
            // Fetch active calls for the number and cancel them
            var calls = await CallResource.ReadAsync(to: new PhoneNumber(dto.MobileNumber), status: CallResource.StatusEnum.InProgress);

            int cancelled = 0;
            foreach (var call in calls)
            {
                await CallResource.UpdateAsync(
                    pathSid: call.Sid,
                    status: CallResource.UpdateStatusEnum.Canceled
                );
                cancelled++;
            }

            return new CallForwardingResultDto
            {
                Success = true,
                Message = cancelled > 0
                    ? $"Call forwarding disabled. {cancelled} active call(s) cancelled for {dto.MobileNumber}"
                    : $"No active forwarded calls found for {dto.MobileNumber}"
            };
        }

        public async Task<CallForwardingResultDto> TestAsync(CallForwardingDto dto)
        {
            // Place a test call that speaks a message then forwards
            var twiml = $@"<Response>
                              <Say>This is a test call forwarding from {dto.MobileNumber}.</Say>
                              <Dial>{dto.ForwardTo}</Dial>
                           </Response>";

            var call = await CallResource.CreateAsync(
                to:   new PhoneNumber(dto.MobileNumber),
                from: new PhoneNumber(_fromNumber),
                twiml: new Twilio.Types.Twiml(twiml)
            );

            return new CallForwardingResultDto
            {
                Success = true,
                CallSid = call.Sid,
                Status  = call.Status.ToString(),
                Message = $"Test call initiated from {dto.MobileNumber} to {dto.ForwardTo}"
            };
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
