//using System.Text.Json;
//using MassTransit;
//using RabbitMQ.Models;
//using RMQPublisherService.Models;
//using RMQPublisherService.Services.Interfaces;

//namespace RMQPublisherService.Services
//{
//    public class SmsService : IPublisherService
//    {
//        private readonly IPublishEndpoint _publishEndpoint;

//        public SmsService(IPublishEndpoint publishEndpoint)
//        {
//            _publishEndpoint = publishEndpoint;
//        }

//        public async Task<ResponseModel> SendMessage1(ReqestEventModel model)
//        {
//            try
//            {
//                var message = new SendMessageModel
//                { 
//                    message = model.message,
//                    mobileNumber = model.mobileNumber,
//                    systemName = model.systemName
//                };

//                await _publishEndpoint.Publish<ISendMessageModel>(message, context =>
//                {
//                    context.SetRoutingKey("message.sms");
//                });

//                return new ResponseModel {
//                    status = 200,
//                    success = true,
//                    message = "Sent message to queue successfully"
//                };
//            }
//            catch (Exception ex)
//            {
//                return new ResponseModel
//                {
//                    status = 500,
//                    message = "Error send message to queue",
//                    error = ex.Message
//                };
//            }
//        }
//    }
//}
