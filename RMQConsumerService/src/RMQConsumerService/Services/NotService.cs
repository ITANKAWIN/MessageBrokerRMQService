using System.Text.Json;
using MassTransit;
using RabbitMQ.Models;
using RMQConsumerService.Models;

namespace RMQConsumerService.Services
{
    public class NotiService : IConsumer<ISendNotiMessage>
    {
        public async Task Consume(ConsumeContext<ISendNotiMessage> context)
        {
            string UserId = "";

            try
            {
                var body = JsonSerializer.Deserialize<SendNotiMessage>(JsonSerializer.Serialize(context.Message));
                if (body == null)
                {
                    throw new Exception();
                }

                UserId = body.UserId.ToString();
                Console.WriteLine($"Received DeviceId: {body.deviceId}, Message: {body.message}");

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Received UserId: {UserId}, Error: {ex.Message}");
            }
        }
    }
}
