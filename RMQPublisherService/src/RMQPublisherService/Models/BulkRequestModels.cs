namespace RMQPublisherService.Models
{
    public class BulkSmsRequestModel
    {
        public int Count { get; set; }
        public string Message { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string SystemName { get; set; } = string.Empty;
    }

    public class BulkNotiRequestModel
    {
        public int Count { get; set; }
        public string Message { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string SystemName { get; set; } = string.Empty;
    }
}
