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
            try
            {
                var aaa = JsonSerializer.Serialize(context.Message);
                Console.WriteLine(aaa);

                //var bbb = JsonSerializer.Deserialize<MessageModel>(aaa);
                //if (bbb == null)
                //{
                //    throw new Exception();
                //}
                //var ccc = JsonSerializer.Deserialize<ReqestSendMessageModel>(bbb.message);
                //if (ccc == null)
                //{
                //    throw new Exception();
                //}

                var bbb = JsonSerializer.Deserialize<SendNotiMessage>(aaa);
                if (bbb == null)
                {
                    throw new Exception();
                }

                Console.WriteLine($"[✔️] Received DeviceId: {bbb.deviceId}, Message: {bbb.message}");

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[X] Received UserId: , Error: {ex.Message}");
            }
        }
    }
}
