using System.Numerics;

namespace RMQPublisherService.Models
{
    public class RabbitMQOption
    {
        public string IP { get; set; } = string.Empty;
        public ushort Port { get; set; } = 5672;
        public string VirtualHost { get; set; } = "/";
        public string Username { get; set; } = string.Empty;
        public string PWD { get; set; } = string.Empty;
        //public string ExchangeName { get; set; } = string.Empty;
        //public string QueueName { get; set; } = string.Empty;
        public string RoutingKey { get; set; } = string.Empty;
        public SmsQueue SmsQueue { get; set; } = new SmsQueue();
        public NotificationQueue NotificationQueue { get; set; } = new NotificationQueue();
    }

    public class SmsQueue : QueueInfo
    {
    }

    public class NotificationQueue : QueueInfo
    {
    }

    public class QueueInfo
    {
        public string ExchangeName { get; set; } = string.Empty;
        public string QueueName { get; set; } = string.Empty;
        public string RoutingKey { get; set; } = string.Empty;
    }
}
