namespace Diagnostic_Lab.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string Message { get; internal set; }
        public string ErrorDetails { get; internal set; }
    }
}
