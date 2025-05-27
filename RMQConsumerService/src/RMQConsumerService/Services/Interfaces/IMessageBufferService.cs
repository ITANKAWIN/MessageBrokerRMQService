namespace RMQConsumerService.Services.Interfaces
{
    using System.Threading.Tasks;
    using RMQConsumerService.Models;

    /// <summary>
    /// Service for buffering messages and performing bulk inserts.
    /// </summary>
    public interface IMessageBufferService
    {
        /// <summary>
        /// Adds an SMS message to the buffer.
        /// </summary>
        Task AddSmsMessageAsync(SendSMSMessage message);

        /// <summary>
        /// Adds a notification message to the buffer.
        /// </summary>
        Task AddNotificationMessageAsync(SendNotiMessage message);

        /// <summary>
        /// Flushes all buffered messages to the database using bulk insert.
        /// </summary>
        Task FlushAsync();
    }
}