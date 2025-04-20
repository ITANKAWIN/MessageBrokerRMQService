using System.Text.Json;
using MassTransit;
using RabbitMQ.Models;
using RMQConsumerService.Models;

namespace RMQConsumerService.Services
{
    public class SmsService : IConsumer<ISendSMSMessage>
    {
        public async Task Consume(ConsumeContext<ISendSMSMessage> context)
        {
            string UserId = "";

            try
            {
                var body = JsonSerializer.Deserialize<SendSMSMessage>(JsonSerializer.Serialize(context.Message));
                if (body == null)
                {
                    throw new Exception();
                }

                UserId = body.UserId.ToString();
                Console.WriteLine($"Received MobileNo: {body.mobileNumber}, Message: {body.message}");

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Received UserId: {UserId}, Error: {ex.Message}");
            }
        }
    }
}
