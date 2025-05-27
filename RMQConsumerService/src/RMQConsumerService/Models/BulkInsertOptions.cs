namespace RMQConsumerService.Models
{
    /// <summary>
    /// Configuration settings for bulk insert operations
    /// This class contains settings to optimize bulk insert performance
    /// </summary>
    public class BulkInsertOptions
    {
        public const string SectionName = "BulkInsert";

        /// <summary>
        /// Default batch size for bulk insert operations
        /// Larger batches provide better performance but use more memory
        /// </summary>
        public int DefaultBatchSize { get; set; } = 1000;

        /// <summary>
        /// Maximum batch size allowed for bulk insert operations
        /// Prevents memory issues with extremely large batches
        /// </summary>
        public int MaxBatchSize { get; set; } = 5000;

        /// <summary>
        /// Timeout in seconds for bulk insert operations
        /// Prevents operations from hanging indefinitely
        /// </summary>
        public int TimeoutSeconds { get; set; } = 300;

        /// <summary>
        /// Enable or disable transaction logging for bulk operations
        /// Disabling can improve performance but reduces debugging capability
        /// </summary>
        public bool EnableTransactionLogging { get; set; } = true;

        /// <summary>
        /// Number of retry attempts for failed bulk insert operations
        /// Helps handle temporary database connection issues
        /// </summary>
        public int RetryAttempts { get; set; } = 3;

        /// <summary>
        /// Delay in milliseconds between retry attempts
        /// Prevents overwhelming the database with rapid retry attempts
        /// </summary>
        public int RetryDelayMs { get; set; } = 1000;

        /// <summary>
        /// Whether to use a temporary database for bulk operations
        /// </summary>
        public bool UseTempDB { get; set; } = false;

        /// <summary>
        /// Batch size for bulk operations
        /// Uses DefaultBatchSize by default
        /// </summary>
        public int BatchSize { get { return DefaultBatchSize; } set { } }

        /// <summary>
        /// Timeout for bulk copy operations in seconds
        /// </summary>
        public int BulkCopyTimeout { get { return TimeoutSeconds; } set { } }

        /// <summary>
        /// Whether to preserve insert order during bulk operations
        /// </summary>
        public bool PreserveInsertOrder { get; set; } = false;

        /// <summary>
        /// Whether to set output identity during bulk operations
        /// </summary>
        public bool SetOutputIdentity { get; set; } = false;

        /// <summary>
        /// Whether to enable streaming for bulk operations
        /// Specific to MySQL provider
        /// </summary>
        public bool EnableStreaming { get; set; } = false;
    }
}