using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using RateLimiter.Contracts;
using Microsoft.Extensions.Logging;
using RateLimiter.Contracts;

namespace RateLimiter.Infrastructure;

/// <summary>
/// Represents a repository for managing blocked requests.
/// </summary>
public class BlockedRequestsRepository : ICacheRepository<string, DateTime, BlockedClientRecord>
{
    private readonly ConcurrentDictionary<string, DateTime> _requests = new();
    private readonly ILogger<BlockedRequestsRepository> _logger;

    /// <summary>
    /// Represents a repository for storing and retrieving blocked client requests.
    /// </summary>
    public BlockedRequestsRepository(ILogger<BlockedRequestsRepository> logger)
    {
        _logger = logger;
    }

    public event DeletedRequestBlockEventHandler? RequestBlockDeleted;

    /// Method: Get
    /// Functionality: Retrieves a BlockedClientRecord if it exists and is not expired.
    /// @param key The key to retrieve the BlockedClientRecord.
    /// @return The BlockedClientRecord associated with the key if it exists and is not expired. Returns null otherwise.
    /// @throws Exception if an error occurs while retrieving the BlockedClientRecord.
    /// /
    public async Task<BlockedClientRecord?> Get(string key)
    {
        return await Task.Run(async () =>
        {
            try
            {
                var found = _requests.TryGetValue(key, out var blockExpiresTime);
                if (!found) return null;
                if (blockExpiresTime.Ticks > DateTime.UtcNow.Ticks)
                    return new BlockedClientRecord(key, blockExpiresTime.ToLocalTime());

                await Delete(key);

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        });
    }

    /// <summary>
    /// Repository for managing blocked requests.
    /// </summary>
    public async Task<IEnumerable<DateTime>> GetMany(string key)
    {
        try
        {
            List<DateTime> blockedRequests = [];
            var blockedRequest = await Get(key);
            if (blockedRequest == null) return null;
            blockedRequests.Add(blockedRequest.BlockExpires);
            return blockedRequests;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
        
    }

    public async Task Add(string key, DateTime value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);
        await AddOrUpdate(key, value);
    }

    /// <summary>
    /// Updates the value of a cache entry associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the cache entry.</param>
    /// <param name="value">The new value to associate with the cache entry.</param>
    /// <returns>A task that represents the asynchronous update operation. The task will complete successfully once the update is done.</returns>
    public async Task Update(string key, DateTime value)
    {
        await AddOrUpdate(key, value);
    }

    /// <summary>
    /// Adds or updates a key-value pair in the BlockedRequestsRepository.
    /// </summary>
    /// <param name="itemKey">The key of the item to add or update.</param>
    /// <param name="value">The value to add or update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the itemKey or value is null.</exception>
    private async Task AddOrUpdate(string itemKey, DateTime value)
    {
        await Task.Run(() =>
        {
            try
            {
                _requests.AddOrUpdate(itemKey,
                    key => value.ToUniversalTime()
                    ,
                    (key, existingVal) => value.ToUniversalTime());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        });
    }

    /// <summary>
    /// Deletes a blocked client record from the repository.
    /// </summary>
    /// <param name="key">The key of the blocked client record to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Delete(string key)
    {
        await Task.Run(() =>
        {
            try
            {
                var wasRemoved = _requests.TryRemove(key, out _);
                if (wasRemoved)
                {
                    _logger.LogInformation($"Removing block for {key}");
                    OnRequestDeleted(new DeletedRequestBlockEventArgs(key));
                }
                else
                {
                    _logger.LogInformation($"Trying to remove block for {key} - NOT FOUND");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        });
    }

    /// <summary>
    /// Represents a repository for managing blocked requests.
    /// </summary>
    protected virtual void OnRequestDeleted(DeletedRequestBlockEventArgs e)
    {
        RequestBlockDeleted?.Invoke(this, e);
    }
}

/// <summary>
/// Represents the event arguments for a deleted request block event.
/// </summary>
public class DeletedRequestBlockEventArgs : EventArgs
{
    public string Key { get; }

    /// <summary>
    /// Represents an event argument for a deleted request block.
    /// </summary>
    public DeletedRequestBlockEventArgs(string key)
    {
        Key = key;
    }
}

/// <summary>
/// Represents a repository for managing blocked requests.
/// </summary>
public delegate void DeletedRequestBlockEventHandler(object sender, DeletedRequestBlockEventArgs e);