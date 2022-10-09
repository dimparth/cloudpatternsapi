using cloudpatternsapi.interfaces;
using cloudpatternsapi.interfaces.services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace cloudpatternsapi.implementation.services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache? _memoryCache;
        private readonly Dictionary<string, TimeSpan> _expirationConfiguration;
        public CacheService(IMemoryCache? memoryCache, Dictionary<string, TimeSpan> expirationConfiguration)
        {
            _memoryCache = memoryCache;
            _expirationConfiguration = expirationConfiguration;
        }
        public void Add<TItem>(TItem item, ICacheKey<TItem> key)
        {
            var cachedObjectName = item!.GetType().Name;
            var timespan = _expirationConfiguration[cachedObjectName];
            _memoryCache.Set(key.CacheKey, item, timespan);
        }

        public TItem? Get<TItem>(ICacheKey<TItem> key) where TItem : class
        {
            if (_memoryCache.TryGetValue(key.CacheKey, out TItem value))
            {
                return value;
            }
            return null;
        }
    }
}
