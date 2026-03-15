using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface ICallForwardingService
    {
        Task<CallForwardingResultDto> EnableAsync(CallForwardingDto dto);
        Task<CallForwardingResultDto> DisableAsync(CallForwardingDto dto);
        Task<CallForwardingResultDto> TestAsync(CallForwardingDto dto);
    }
}
