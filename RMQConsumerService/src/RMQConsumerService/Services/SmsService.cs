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

                var bbb = JsonSerializer.Deserialize<SendSMSMessage>(aaa);
                if (bbb == null)
                {
                    throw new Exception();
                }

                Console.WriteLine($"[✔️] Received MobileNo: {bbb.mobileNumber}, Message: {bbb.message}");

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[X] Received UserId: , Error: {ex.Message}");
            }
        }
    }
}
