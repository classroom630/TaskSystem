namespace TaskSystem.API.Models.DTOs
{
    public class ErrorResponseDto
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    public class ValidationErrorResponseDto : ErrorResponseDto
    {
        public Dictionary<string, string[]> Errors { get; set; } = new();
    }
}