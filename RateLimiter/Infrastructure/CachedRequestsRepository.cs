using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using RateLimiter.Contracts;
using Microsoft.Extensions.Logging;

namespace RateLimiter.Infrastructure;

/// <summary>
/// The `CachedRequestsRepository` class implements the `ICacheRepository` interface and provides methods for caching and retrieving request details.
/// </summary>
public class CachedRequestsRepository : ICacheRepository<string, RequestDetails, CachedRequestsRecord>
{
    private readonly ILogger<CachedRequestsRepository> _logger;
    private readonly ConcurrentDictionary<string, CachedRequestsRecord> _requests = new();

    /// <summary>
    /// The <see cref="CachedRequestsRepository"/> class implements the <see cref="ICacheRepository{TKey, TValue, TReturnType}"/> interface and provides methods for caching and retrieving request details.
    /// </summary>
    public CachedRequestsRepository(ILogger<CachedRequestsRepository> logger)
    {
        _logger = logger;
    }

    /// Retrieves a <see cref="CachedRequestsRecord"/> for the specified key.
    /// </summary>
    /// <param name="key">The key to retrieve the record for.</param>
    /// <returns>The <see cref="CachedRequestsRecord"/> for the specified key, if it exists; otherwise, <see langword="null"/>.</returns>
    public async Task<CachedRequestsRecord?> Get(string key)
    {
        return await Task.Run(() =>
        {
            try
            {
                _requests.TryGetValue(key, out var record);
                return record;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        });
    }

    /// <summary>
    /// Retrieves multiple <see cref="RequestDetails"/> objects from the cache based on the specified key.
    /// </summary>
    /// <param name="key">The key used to retrieve the cached requests.</param>
    /// <returns>A collection of <see cref="RequestDetails"/> objects if the key exists in the cache; otherwise, <c>null</c>.</returns>
    public async Task<IEnumerable<RequestDetails>?> GetMany(string key)
    {
        try
        {
            var cachedRequest = await Get(key);
            var requests = cachedRequest?.Requests.ToArray();
            return requests;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    /// <summary>
    /// Adds a request to the CachedRequestsRepository.
    /// </summary>
    /// <param name="key">The key associated with the request in the repository.</param>
    /// <param name="request">The request to be added.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Add(string key, RequestDetails request)
    {
        await AddOrUpdate(key, request);
    }

    /// <summary>
    /// The <c>CachedRequestsRepository</c> class implements the <c>ICacheRepository</c> interface and provides methods for caching and retrieving request details.
    /// </summary>
    public async Task Update(string key, RequestDetails value)
    {
        await AddOrUpdate(key, value);
    }

    /// <summary>
    /// The `AddOrUpdate` method adds or updates a request in the cached requests repository.
    /// </summary>
    /// <param name="itemKey">The key of the request.</param>
    /// <param name="value">The request details to add or update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task AddOrUpdate(string itemKey, RequestDetails value)
    {
        await Task.Run(() =>
        {
            try
            {
                _requests.AddOrUpdate(itemKey,
                    key =>
                    {
                        var queue = new Queue<RequestDetails>();
                        queue.Enqueue(value);
                        return new CachedRequestsRecord(value.RequestId, queue);
                    }
                    ,
                    (key, existingVal) =>
                    {
                        existingVal.Requests.Enqueue(value);
                        return existingVal;
                    });
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        });
    }

    /// <summary>
    /// Deletes a request record from the cached requests repository.
    /// </summary>
    /// <param name="key">The key of the request record to delete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task Delete(string key)
    {
        try
        {
            _requests.TryRemove(key, out _);
            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw;
        }
    }


    /// Handles the deletion of a request block by invoking the necessary actions asynchronously.
    /// </summary>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">The arguments for the deleted request block event.</param>
    public void HandleBlockDeleted(object sender, DeletedRequestBlockEventArgs e)
    {
        _ = Task.Run(async () => { await Delete(e.Key); });
    }
}