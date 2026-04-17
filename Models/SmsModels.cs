namespace WebApplication1.Models
{
    public class SmsDto
    {
        public required string To { get; set; }
        public required string Message { get; set; }
    }

    public class SmsBulkDto
    {
        public required List<string> Numbers { get; set; }
        public required string Message { get; set; }
    }

    public class SmsResultDto
    {
        public bool Success { get; set; }
        public string? MessageSid { get; set; }
        public string? Status { get; set; }
        public required string Message { get; set; }
    }

    public class SmsBulkResultDto
    {
        public int TotalSent { get; set; }
        public int TotalFailed { get; set; }
        public required List<SmsResultDto> Results { get; set; }
    }
}
