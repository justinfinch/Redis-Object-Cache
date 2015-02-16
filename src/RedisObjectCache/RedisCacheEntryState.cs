using System;
using System.Runtime.Caching;

namespace RedisObjectCache
{
    internal class RedisCacheEntryState
    {
        public DateTime UtcCreated { get; set; }
        public DateTime UtcAbsoluteExpiration { get; set; }
        public DateTime UtcLastUpdateUsage { get; set; }

        public DateTimeOffset AbsoluteExpiration { get; set; }
        public TimeSpan SlidingExpiration { get; set; }
        public string Priority { get; set; }
        public string TypeName { get; set; }

        public bool IsSliding
        {
            get { return SlidingExpiration > TimeSpan.Zero; }
        }

        internal RedisCacheEntryState()
        {
            
        }

        internal RedisCacheEntryState(DateTimeOffset absExp,
                                  TimeSpan slidingExp,
                                  CacheItemPriority priority,
                                  string typeName)
        {
            AbsoluteExpiration = absExp;
            SlidingExpiration = slidingExp;
            Priority = priority.ToString();
            TypeName = typeName;

            UtcCreated = DateTime.UtcNow;

            UpdateUsage();
        }

        internal void UpdateUsage()
        {
            UtcLastUpdateUsage = DateTime.UtcNow;
            if (IsSliding)
            {
                UtcAbsoluteExpiration = UtcLastUpdateUsage + SlidingExpiration;
            }
            else
            {
                UtcAbsoluteExpiration = AbsoluteExpiration.UtcDateTime;
            }
        }
    }
}
