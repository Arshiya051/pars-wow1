using Microsoft.Extensions.Caching.Memory;
using ParsWoW.Api.Application.Abstractions.Common;

namespace ParsWoW.Api.Infrastructure.Cache;

/// <summary>
/// Thin wrapper over <see cref="IMemoryCache"/> providing typed get-or-create
/// semantics and explicit invalidation. <see cref="ICachingService"/> is the
/// abstraction; the implementation lives here.
/// </summary>
public sealed class MemoryCachingService : ICachingService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCachingService> _logger;

    public MemoryCachingService(IMemoryCache cache, ILogger<MemoryCachingService> logger)
    {
        _cache = cache; _logger = logger;
    }

    public TValue? Get<TValue>(string key) => _cache.TryGetValue(key, out var v) ? (TValue?)v : default;

    public bool TryGet<TValue>(string key, out TValue? value)
    {
        if (_cache.TryGetValue(key, out var raw))
        {
            value = (TValue?)raw;
            return value is not null || raw is null;
        }
        value = default;
        return false;
    }

    public async Task<TValue> GetOrCreateAsync<TValue>(
        string key,
        Func<CancellationToken, Task<TValue>> factory,
        TimeSpan? absoluteExpirationRelativeToNow = null,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGet<TValue>(key, out var existing) && existing is not null)
            return existing;

        var value = await factory(cancellationToken).ConfigureAwait(false);

        var options = new MemoryCacheEntryOptions();
        if (absoluteExpirationRelativeToNow is { } abs) options.AbsoluteExpirationRelativeToNow = abs;
        if (slidingExpiration is { } slide) options.SlidingExpiration = slide;
        if (absoluteExpirationRelativeToNow is null && slidingExpiration is null)
            options.SlidingExpiration = TimeSpan.FromMinutes(15);

        _cache.Set(key, value, options);
        _logger.LogDebug("Cached {Key} ({Type})", key, typeof(TValue).Name);
        return value;
    }

    public void Set<TValue>(string key, TValue value, TimeSpan? ttl = null)
    {
        var options = new MemoryCacheEntryOptions();
        if (ttl is { } t) options.AbsoluteExpirationRelativeToNow = t;
        _cache.Set(key, value, options);
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        _logger.LogDebug("Evicted {Key}", key);
    }

    public void RemoveByPrefix(string prefix)
    {
        // IMemoryCache has no native prefix scan; rely on compaction via size limit.
        // For per-DBC re-load we explicitly enumerate registered keys per provider.
        _logger.LogDebug("RemoveByPrefix called for {Prefix} (no-op in IMemoryCache)", prefix);
    }
}
