using RabbitMQ.Models;
using RMQPublisherService.Models;

namespace RMQPublisherService.Services.Interfaces
{
    public interface IPublisherService
    {
        Task<ResponseModel> SendSMS(ReqestSMSEventModel model);
        Task<ResponseModel> SendNoti(ReqestNotiEventModel model);
    }
}
