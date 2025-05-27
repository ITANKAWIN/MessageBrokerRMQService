using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using RMQConsumerService.Data;
using RMQConsumerService.Models;
using RMQConsumerService.Services.Interfaces;

namespace RMQConsumerService.Services
{
    /// <summary>
    /// Implementation of bulk insert operations into MySQL database
    /// </summary>
    public class LogApplication : ILogApplication
    {
        private readonly AppDbContext _context;
        private readonly BulkInsertOptions _options;
        private readonly ILogger<LogApplication> _logger;

        public LogApplication(
            AppDbContext context,
            IOptions<BulkInsertOptions> options,
            ILogger<LogApplication> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private BulkConfig CreateBulkConfig<T>() where T : class
        {
            return new BulkConfig
            {
                // Set bulk config options from the injected settings
                UseTempDB = _options.UseTempDB,
                BatchSize = _options.BatchSize,
                BulkCopyTimeout = _options.BulkCopyTimeout,

                // Enable or disable specific features
                PreserveInsertOrder = _options.PreserveInsertOrder,
                SetOutputIdentity = _options.SetOutputIdentity,

                // MySQL specific options if needed
                EnableStreaming = _options.EnableStreaming
            };
        }

        public async Task BulkInsertAsync<T>(IEnumerable<T> entities) where T : class
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            var list = entities as IList<T> ?? entities.ToList();
            if (!list.Any())
            {
                _logger.LogInformation("No entities to bulk insert for {EntityName}.", typeof(T).Name);
                return;
            }

            int attempt = 0;
            while (true)
            {
                try
                {
                    attempt++;
                    _logger.LogInformation("Starting bulk insert for {EntityName}, Count={Count}, Attempt={Attempt}.", typeof(T).Name, list.Count, attempt);
                    var config = CreateBulkConfig<T>();

                    try
                    {
                        // When UseTempDB is true, we need to use a transaction
                        if (_options.EnableTransactionLogging)
                        {
                            await using var transaction = await _context.Database.BeginTransactionAsync();
                            try
                            {
                                // Attempt bulk insert using EFCore.BulkExtensions within transaction
                                await _context.BulkInsertAsync(list, config);
                                await transaction.CommitAsync();
                                _logger.LogInformation("Bulk insert with transaction completed successfully for {EntityName}.", typeof(T).Name);
                                break;
                            }
                            catch
                            {
                                await transaction.RollbackAsync();
                                throw; // Rethrow to be caught by outer catch block
                            }
                        }
                        else
                        {
                            // If not using temp DB, we can perform bulk insert without transaction
                            await _context.BulkInsertAsync(list, config);
                            _logger.LogInformation("Bulk insert completed successfully for {EntityName}.", typeof(T).Name);
                            break;
                        }
                    }
                    catch (Exception dbEx) when (
                        dbEx is InvalidOperationException ||
                        dbEx is NotSupportedException)
                    {
                        // Check for all known bulk insert issues
                        bool isKnownBulkError =
                            dbEx.Message.Contains("Failed to create DbServer") ||
                            dbEx.Message.Contains("When 'UseTempDB' is set then BulkOperation has to be inside Transaction") ||
                            dbEx.Message.Contains("AllowLoadLocalInfile=true") ||
                            dbEx.Message.Contains("To use MySqlBulkLoader.Local=true");

                        if (isKnownBulkError)
                        {
                            // Fallback to regular AddRange if bulk insert fails due to provider issues
                            _logger.LogWarning("Bulk insert issue detected: {ErrorMessage}. Falling back to standard EF Core insert for {EntityName}.",
                                dbEx.Message, typeof(T).Name);

                            await using var transaction = await _context.Database.BeginTransactionAsync();
                            try
                            {
                                _context.Set<T>().AddRange(list);
                                await _context.SaveChangesAsync();
                                await transaction.CommitAsync();
                                _logger.LogInformation("Standard insert completed successfully for {EntityName}.", typeof(T).Name);
                                break;
                            }
                            catch (Exception fallbackEx)
                            {
                                await transaction.RollbackAsync();

                                // Second fallback: try inserting records one by one if batch insert fails
                                if (fallbackEx is DbUpdateException)
                                {
                                    _logger.LogWarning("Batch insert failed, attempting individual inserts for {EntityName}.", typeof(T).Name);
                                    bool success = true;

                                    foreach (var entity in list)
                                    {
                                        try
                                        {
                                            await using var individualTransaction = await _context.Database.BeginTransactionAsync();
                                            _context.Set<T>().Add(entity);
                                            await _context.SaveChangesAsync();
                                            await individualTransaction.CommitAsync();
                                        }
                                        catch (Exception individualEx)
                                        {
                                            _logger.LogError(individualEx, "Failed to insert individual entity of type {EntityName}.", typeof(T).Name);
                                            success = false;
                                        }
                                    }

                                    if (success)
                                    {
                                        _logger.LogInformation("Individual inserts completed successfully for {EntityName}.", typeof(T).Name);
                                        break;
                                    }
                                }

                                _logger.LogError(fallbackEx, "All insert methods failed for {EntityName}.", typeof(T).Name);
                                throw;
                            }
                        }
                        else
                        {
                            // If it's not a known bulk error, just rethrow
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Bulk insert failed for {EntityName} on attempt {Attempt}.", typeof(T).Name, attempt);
                    if (attempt >= _options.RetryAttempts)
                    {
                        _logger.LogError("Reached max retry attempts ({RetryAttempts}) for {EntityName}.", _options.RetryAttempts, typeof(T).Name);
                        throw;
                    }

                    _logger.LogInformation("Waiting {RetryDelayMs}ms before next retry.", _options.RetryDelayMs);
                    await Task.Delay(_options.RetryDelayMs);
                }
            }
        }
    }
}