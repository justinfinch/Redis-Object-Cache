using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace RedisObjectCache
{
    internal class RedisCacheEntryState
    {
        internal DateTime UtcCreated { get; private set; }
        internal DateTime UtcAbsoluteExpiration { get; private set; }
        internal DateTime UtcLastUpdateUsage { get; private set; }

        internal DateTimeOffset AbsoluteExpiration { get; private set; }
        internal TimeSpan SlidingExpiration { get; private set; }
        internal string Priority { get; private set; }

        public bool IsSliding
        {
            get { return SlidingExpiration > TimeSpan.Zero; }
        }

        internal RedisCacheEntryState(DateTimeOffset absExp,
                                  TimeSpan slidingExp,
                                  CacheItemPriority priority)
        {
            AbsoluteExpiration = absExp;
            SlidingExpiration = slidingExp;
            Priority = priority.ToString();

            UtcCreated = UtcLastUpdateUsage = DateTime.UtcNow;

            UpdateUsage();
        }

        internal void UpdateUsage()
        {
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
