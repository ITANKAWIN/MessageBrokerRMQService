using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RMQConsumerService.Models;
using RMQConsumerService.Services.Interfaces;

namespace RMQConsumerService.Services
{
    /// <summary>
    /// Service for buffering messages and performing bulk inserts.
    /// </summary>
    public class MessageBufferService : IMessageBufferService
    {
        private readonly object _lock = new object();
        private readonly List<SendSMSMessage> _smsBuffer = new List<SendSMSMessage>();
        private readonly List<SendNotiMessage> _notiBuffer = new List<SendNotiMessage>();
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly BulkInsertOptions _options;
        private readonly ILogger<MessageBufferService> _logger;

        public MessageBufferService(
            IServiceScopeFactory scopeFactory,
            IOptions<BulkInsertOptions> options,
            ILogger<MessageBufferService> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddSmsMessageAsync(SendSMSMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            List<SendSMSMessage>? toFlush = null;
            lock (_lock)
            {
                _smsBuffer.Add(message);
                _logger.LogInformation("SMS buffer count: {Count}", _smsBuffer.Count);

                if (_smsBuffer.Count >= _options.DefaultBatchSize)
                {
                    toFlush = new List<SendSMSMessage>(_smsBuffer);
                    _smsBuffer.Clear();
                }
            }

            if (toFlush != null)
            {
                _logger.LogInformation("Flushing SMS buffer with {Count} items.", toFlush.Count);
                using var scope = _scopeFactory.CreateScope();
                var logApp = scope.ServiceProvider.GetRequiredService<ILogApplication>();
                await logApp.BulkInsertAsync(toFlush);
            }
        }

        public async Task AddNotificationMessageAsync(SendNotiMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            List<SendNotiMessage>? toFlush = null;
            lock (_lock)
            {
                _notiBuffer.Add(message);
                _logger.LogInformation("Notification buffer count: {Count}", _notiBuffer.Count);

                if (_notiBuffer.Count >= _options.DefaultBatchSize)
                {
                    toFlush = new List<SendNotiMessage>(_notiBuffer);
                    _notiBuffer.Clear();
                }
            }

            if (toFlush != null)
            {
                _logger.LogInformation("Flushing notification buffer with {Count} items.", toFlush.Count);
                using var scope = _scopeFactory.CreateScope();
                var logApp = scope.ServiceProvider.GetRequiredService<ILogApplication>();
                await logApp.BulkInsertAsync(toFlush);
            }
        }

        public async Task FlushAsync()
        {
            List<SendSMSMessage> smsToFlush;
            List<SendNotiMessage> notiToFlush;

            lock (_lock)
            {
                smsToFlush = new List<SendSMSMessage>(_smsBuffer);
                _smsBuffer.Clear();
                notiToFlush = new List<SendNotiMessage>(_notiBuffer);
                _notiBuffer.Clear();
            }

            if (smsToFlush.Count > 0)
            {
                _logger.LogInformation("Flushing SMS buffer with {Count} items.", smsToFlush.Count);
                using var scope = _scopeFactory.CreateScope();
                var logApp = scope.ServiceProvider.GetRequiredService<ILogApplication>();
                await logApp.BulkInsertAsync(smsToFlush);
            }

            if (notiToFlush.Count > 0)
            {
                _logger.LogInformation("Flushing notification buffer with {Count} items.", notiToFlush.Count);
                using var scope = _scopeFactory.CreateScope();
                var logApp = scope.ServiceProvider.GetRequiredService<ILogApplication>();
                await logApp.BulkInsertAsync(notiToFlush);
            }
        }
    }
}
