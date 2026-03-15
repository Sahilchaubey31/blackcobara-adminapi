namespace WebApplication1.Models
{
    public class CallForwardingDto
    {
        public required string MobileNumber { get; set; }
        public required string ForwardTo { get; set; }
        public string Type { get; set; } = "Always"; // Always, Busy, NoAnswer
        public int NoAnswerSeconds { get; set; } = 20;
    }

    public class CallForwardingResultDto
    {
        public bool Success { get; set; }
        public string? CallSid { get; set; }
        public string? Status { get; set; }
        public required string Message { get; set; }
    }
}
