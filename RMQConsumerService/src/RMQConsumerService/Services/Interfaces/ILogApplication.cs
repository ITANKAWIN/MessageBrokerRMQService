namespace RMQConsumerService.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for bulk insert operations into the database
    /// </summary>
    public interface ILogApplication
    {
        /// <summary>
        /// Bulk inserts a collection of entities into the database
        /// </summary>
        Task BulkInsertAsync<T>(IEnumerable<T> entities) where T : class;
    }
}