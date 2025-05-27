using RabbitMQ.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RMQConsumerService.Models
{
    public class ReqestSMSEventModel : SendSMSMessage
    {
    }

    public class ReqestNotiEventModel : SendNotiMessage
    {
    }

    [Table("SMSMessages")]
    public class SendSMSMessage : ISendSMSMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        
        public string message { get; set; } = string.Empty;
        public string mobileNumber { get; set; } = string.Empty;
        public string systemName { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ProcessedAt { get; set; }
    }

    [Table("NotificationMessages")]
    public class SendNotiMessage : ISendNotiMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        
        public string message { get; set; } = string.Empty;
        public string deviceId { get; set; } = string.Empty;
        public string systemName { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ProcessedAt { get; set; }
    }
}
