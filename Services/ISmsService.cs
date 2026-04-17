using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface ISmsService
    {
        Task<SmsResultDto> SendAsync(SmsDto dto);
        Task<SmsBulkResultDto> SendBulkAsync(SmsBulkDto dto);
    }
}
