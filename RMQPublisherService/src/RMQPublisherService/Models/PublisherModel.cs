using RabbitMQ.Models;

namespace RMQPublisherService.Models
{
    public class ReqestSMSEventModel : SendSMSMessage
    {
    }

    public class ReqestNotiEventModel : SendNotiMessage
    {
    }

    public class SendSMSMessage : ISendSMSMessage
    {
        public string message { get; set; } = string.Empty;
        public string mobileNumber { get; set; } = string.Empty;
        public string systemName { get; set; } = string.Empty;
        public Guid UserId { get; set; }
    }

    public class SendNotiMessage : ISendNotiMessage
    {
        public string message { get; set; } = string.Empty;
        public string deviceId { get; set; } = string.Empty;
        public string systemName { get; set; } = string.Empty;
        public Guid UserId { get; set; }
    }
}
