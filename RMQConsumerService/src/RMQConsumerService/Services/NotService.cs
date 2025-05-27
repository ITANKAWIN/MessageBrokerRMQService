using System.Text.Json;
using MassTransit;
using RabbitMQ.Models;
using RMQConsumerService.Data;
using RMQConsumerService.Models;
using RMQConsumerService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace RMQConsumerService.Services
{    public class NotiService : IConsumer<ISendNotiMessage>
    {
        private readonly IMessageBufferService _messageBufferService;
        private readonly ILogger<NotiService> _logger;

        public NotiService(IMessageBufferService messageBufferService, ILogger<NotiService> logger)
        {
            _messageBufferService = messageBufferService ?? throw new ArgumentNullException(nameof(messageBufferService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }        public async Task Consume(ConsumeContext<ISendNotiMessage> context)
        {
            string UserId = "";

            try
            {
                var body = JsonSerializer.Deserialize<SendNotiMessage>(JsonSerializer.Serialize(context.Message));
                if (body == null)
                {
                    throw new Exception("Failed to deserialize notification message");
                }

                UserId = body.UserId.ToString();
                _logger.LogInformation("Processing notification message - DeviceId: {DeviceId}, Message: {Message}, UserId: {UserId}", 
                    body.deviceId, body.message, UserId);

                // Create notification record for buffering
                var notiRecord = new SendNotiMessage
                {
                    UserId = body.UserId,
                    message = body.message,
                    deviceId = body.deviceId,
                    systemName = body.systemName,
                    CreatedAt = DateTime.UtcNow
                    // Note: ProcessedAt will be set when the message is actually processed from buffer
                };

                // Add to buffer for bulk insert instead of immediate database insert
                await _messageBufferService.AddNotificationMessageAsync(notiRecord);

                _logger.LogInformation("Notification message added to bulk insert buffer successfully. UserId: {UserId}", UserId);

                // Here you would implement the actual push notification sending logic
                // For example: await _pushNotificationService.SendNotificationAsync(body.deviceId, body.message);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification message. UserId: {UserId}", UserId);
                throw; // Re-throw to allow MassTransit to handle retry logic
            }
        }
    }
}
