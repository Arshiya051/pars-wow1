namespace ParsWoW.Api.Application.Abstractions.Common;

/// <summary>
/// Abstraction over the caching layer. The default implementation is
/// <c>Infrastructure.Cache.MemoryCachingService</c> backed by
/// <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache"/>.
/// </summary>
public interface ICachingService
{
    TValue? Get<TValue>(string key);
    bool TryGet<TValue>(string key, out TValue? value);

    Task<TValue> GetOrCreateAsync<TValue>(
        string key,
        Func<CancellationToken, Task<TValue>> factory,
        TimeSpan? absoluteExpirationRelativeToNow = null,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default);

    void Set<TValue>(string key, TValue value, TimeSpan? ttl = null);
    void Remove(string key);
    void RemoveByPrefix(string prefix);
}
