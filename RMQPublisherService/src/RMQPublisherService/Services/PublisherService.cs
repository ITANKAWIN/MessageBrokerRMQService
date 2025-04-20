using MassTransit;
using RabbitMQ.Models;
using RMQPublisherService.Models;
using RMQPublisherService.Services.Interfaces;

namespace RMQPublisherService.Services
{
    public class PublisherService : IPublisherService
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public PublisherService(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task<ResponseModel> SendSMS(ReqestSMSEventModel model)
        {
            try
            {
                var message = new SendSMSMessage { message = model.message, mobileNumber = model.mobileNumber, systemName = model.systemName, UserId = Guid.NewGuid() };
                await _publishEndpoint.Publish<ISendSMSMessage>(message, context =>
                {
                    context.SetRoutingKey("message.sms");
                });

                return new ResponseModel {
                    status = 200,
                    success = true,
                    message = "Sent message to queue successfully"
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    status = 500,
                    message = "Error send message to queue",
                    error = ex.Message
                };
            }
        }

        public async Task<ResponseModel> SendNoti(ReqestNotiEventModel model)
        {
            try
            {
                var message = new SendNotiMessage { message = model.message, deviceId = model.deviceId, systemName = model.systemName, UserId = Guid.NewGuid() };
                await _publishEndpoint.Publish<ISendNotiMessage>(message, context =>
                {
                    context.SetRoutingKey("message.notification");
                });

                return new ResponseModel
                {
                    status = 200,
                    success = true,
                    message = "Sent message to queue successfully"
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    status = 500,
                    message = "Error send message to queue",
                    error = ex.Message
                };
            }
        }
    }
}
