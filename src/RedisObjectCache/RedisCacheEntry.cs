using System;
using System.Runtime.Caching;

namespace RedisObjectCache
{
    internal sealed class RedisCacheEntry
    {
        public string Key { get; set; }
        public object ItemValue { get; set; }
        public string ItemType { get; set; }

        public RedisCacheEntryState State { get; set; }

        public RedisCacheEntry(String key,
                                  Object value,
                                  DateTimeOffset absoluteExpiration,
                                  TimeSpan slidingExpiration,
                                  CacheItemPriority priority)
            : this(key, value, absoluteExpiration, slidingExpiration, priority.ToString())
        {
        }

        public RedisCacheEntry(String key,
                          Object value,
                          DateTimeOffset absoluteExpiration,
                          TimeSpan slidingExpiration,
                          string priority)
        {
            Key = key;
            ItemValue = value;
            ItemType = value.GetType().AssemblyQualifiedName;
            State = new RedisCacheEntryState(absoluteExpiration, slidingExpiration, priority);
        }
    }
}
