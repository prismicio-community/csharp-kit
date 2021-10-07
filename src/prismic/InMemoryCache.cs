using System;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace prismic
{
    public class InMemoryCache : ICache
    {
        private readonly IMemoryCache _memoryCache;

        public InMemoryCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void Set(string key, long ttl, JToken item)
            => _memoryCache.Set(key, item, TimeSpan.FromSeconds(ttl));

        public JToken Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            return _memoryCache.TryGetValue(key, out JToken entry)
                ? entry
                : null;
        }

        public Task<T> GetOrSetAsync<T>(string key, long ttl, Func<Task<T>> factory)
        {
            if (string.IsNullOrWhiteSpace(key))
                return Task.FromResult<T>(default);

            return _memoryCache.GetOrCreateAsync(
                key,
                (entry) =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(ttl);
                    return factory();
                }
            );
        }
    }
}
