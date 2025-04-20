namespace RMQPublisherService.Models
{
    public class ResponseModel
    {
        public int status { get; set; }
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
        public object? error { get; set; }
    }
}
