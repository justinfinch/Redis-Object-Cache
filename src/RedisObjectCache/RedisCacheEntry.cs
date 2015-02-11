using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace RedisObjectCache
{
    internal sealed class RedisCacheEntry : RedisCacheKey
    {
        public object Value { get; set; }
        public RedisCacheEntryState State { get; set; }

        internal RedisCacheEntry(String key,
                                  Object value,
                                  DateTimeOffset absExp,
                                  TimeSpan slidingExp,
                                  CacheItemPriority priority)
            : base(key)
        {
            Value = value;
            State = new RedisCacheEntryState(absExp, slidingExp, priority);
        }
    }
}
