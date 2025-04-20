namespace RabbitMQ.Models
{
    public interface ISendSMSMessage
    {
        public Guid UserId { get; set; }
        public string message { get; set; }
        public string mobileNumber { get; set; }
        public string systemName { get; set; }
    }

    public interface ISendNotiMessage
    {
        public Guid UserId { get; set; }
        public string message { get; set; }
        public string deviceId { get; set; }
        public string systemName { get; set; }
    }
}
