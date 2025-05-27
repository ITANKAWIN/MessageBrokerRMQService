using System.Text.Json;
using MassTransit;
using RabbitMQ.Models;
using RMQConsumerService.Models;
using RMQConsumerService.Data;
using RMQConsumerService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace RMQConsumerService.Services
{    public class SmsService : IConsumer<ISendSMSMessage>
    {
        private readonly IMessageBufferService _messageBufferService;
        private readonly ILogger<SmsService> _logger;

        public SmsService(IMessageBufferService messageBufferService, ILogger<SmsService> logger)
        {
            _messageBufferService = messageBufferService ?? throw new ArgumentNullException(nameof(messageBufferService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }        public async Task Consume(ConsumeContext<ISendSMSMessage> context)
        {
            string UserId = "";

            try
            {
                var body = JsonSerializer.Deserialize<SendSMSMessage>(JsonSerializer.Serialize(context.Message));
                if (body == null)
                {
                    throw new Exception("Failed to deserialize SMS message");
                }

                UserId = body.UserId.ToString();
                _logger.LogInformation("Processing SMS message - MobileNo: {MobileNumber}, Message: {Message}, UserId: {UserId}", 
                    body.mobileNumber, body.message, UserId);

                // Create SMS record for buffering
                var smsRecord = new SendSMSMessage
                {
                    UserId = body.UserId,
                    message = body.message,
                    mobileNumber = body.mobileNumber,
                    systemName = body.systemName,
                    CreatedAt = DateTime.UtcNow
                    // Note: ProcessedAt will be set when the message is actually processed from buffer
                };

                // Add to buffer for bulk insert instead of immediate database insert
                await _messageBufferService.AddSmsMessageAsync(smsRecord);

                _logger.LogInformation("SMS message added to bulk insert buffer successfully. UserId: {UserId}", UserId);

                // Here you would implement the actual SMS sending logic
                // For example: await _smsProvider.SendSmsAsync(body.mobileNumber, body.message);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SMS message. UserId: {UserId}", UserId);
                throw; // Re-throw to allow MassTransit to handle retry logic
            }
        }
    }
}
