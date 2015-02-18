using System;
using System.Runtime.Caching;
using Newtonsoft.Json;

namespace RedisObjectCache
{
    internal class RedisCacheEntryState
    {
        public DateTimeOffset AbsoluteExpiration { get; set; }
        public TimeSpan SlidingExpiration { get; set; }
        public string Priority { get; set; }

        [JsonIgnore]
        public bool IsSliding
        {
            get { return SlidingExpiration > TimeSpan.Zero; }
        }

        public TimeSpan GetTtl()
        {
            var absoluteExpirationTimespan = AbsoluteExpiration.Subtract(DateTime.UtcNow);

            var ttl = absoluteExpirationTimespan > SlidingExpiration && IsSliding
                ? SlidingExpiration
                : absoluteExpirationTimespan;

            if (ttl <= TimeSpan.Zero)
            {
                ttl = TimeSpan.FromSeconds(1); //There is a minimum expiry time of 1 second
            }

            return ttl;
        }

        internal RedisCacheEntryState()
        {
            
        }

        internal RedisCacheEntryState(DateTimeOffset absoluteExpiration,
                          TimeSpan slidingExpiration,
                          string priority)
        {
            AbsoluteExpiration = absoluteExpiration;
            SlidingExpiration = slidingExpiration;
            Priority = priority;
        }
    }
}
