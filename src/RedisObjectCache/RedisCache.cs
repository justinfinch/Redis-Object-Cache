using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisObjectCache
{
    public class RedisCache : ObjectCache, IDisposable
    {
        private const string REGION_NOT_SUPPORTED = "Region not supported";

        private const DefaultCacheCapabilities CAPABILITIES = DefaultCacheCapabilities.AbsoluteExpirations
                                                              | DefaultCacheCapabilities.SlidingExpirations;

         // static holder for instance, need to use lambda to construct since constructor private
        private static readonly Lazy<RedisCache> _instance = new Lazy<RedisCache>(() => new RedisCache());
        public static RedisCache Default
        {
            get { return _instance.Value; }
        }

        private static readonly TimeSpan OneYear = new TimeSpan(365, 0, 0, 0);
        private readonly RedisCacheStore _store;

        private int _disposed;
        private bool IsDisposed { get { return (_disposed == 1); } }

        private RedisCache()
        {
            var connectionSettings = RedisCacheConfiguration.Instance.Connection;

            var configurationOptions = new ConfigurationOptions
            {
                EndPoints = {{ connectionSettings.Host, connectionSettings.Port }},
                ConnectTimeout = connectionSettings.ConnectionTimeoutInMilliseconds,
                Password = connectionSettings.AccessKey,
                Ssl = connectionSettings.Ssl,
                SyncTimeout = connectionSettings.OperationTimeoutInMilliseconds

            };

            var redis = ConnectionMultiplexer.Connect(configurationOptions);
            var database = redis.GetDatabase(RedisCacheConfiguration.Instance.Connection.DatabaseId);
            _store = new RedisCacheStore(database);
        }

        public override object AddOrGetExisting(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException(REGION_NOT_SUPPORTED);
            }

            return AddOrGetExistingInternal(key, value, policy);
        }

        public override CacheItem AddOrGetExisting(CacheItem value, CacheItemPolicy policy)
        {
            AddOrGetExistingInternal(value.Key, value.Value, policy);
            return value;
        }

        public override object AddOrGetExisting(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException(REGION_NOT_SUPPORTED);
            }

            var policy = new CacheItemPolicy {AbsoluteExpiration = absoluteExpiration};
            return AddOrGetExistingInternal(key, value, policy);
        }

        public override bool Contains(string key, string regionName = null)
        {
            return (GetInternal(key, regionName) != null);
        }

        public override CacheEntryChangeMonitor CreateCacheEntryChangeMonitor(IEnumerable<string> keys, string regionName = null)
        {
            throw new NotImplementedException();
        }

        public override DefaultCacheCapabilities DefaultCacheCapabilities
        {
            get { return CAPABILITIES; }
        }

        public override object Get(string key, string regionName = null)
        {
            return GetInternal(key, regionName);
        }

        public override CacheItem GetCacheItem(string key, string regionName = null)
        {
            object value = GetInternal(key, regionName);
            return (value != null) ? new CacheItem(key, value) : null;
        }

        public override long GetCount(string regionName = null)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> GetValues(IEnumerable<string> keys, string regionName = null)
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get { return "Redis Default"; }
        }

        public override object Remove(string key, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException(REGION_NOT_SUPPORTED);
            }

            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (IsDisposed)
            {
                return null;
            }

            return _store.Remove(key);
        }

        public override void Set(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException(REGION_NOT_SUPPORTED);
            }

            AddOrGetExistingInternal(key, value, policy);
        }

        public override void Set(CacheItem item, CacheItemPolicy policy)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            Set(item.Key, item.Value, policy);
        }

        public override void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException(REGION_NOT_SUPPORTED);
            }
            CacheItemPolicy policy = new CacheItemPolicy
            {
                AbsoluteExpiration = absoluteExpiration
            };
            Set(key, value, policy);
        }

        public override object this[string key]
        {
            get
            {
                return GetInternal(key, null);
            }
            set
            {
                Set(key, value, InfiniteAbsoluteExpiration);
            }
        }

        private object GetInternal(string key, string regionName)
        {
            if (IsDisposed)
            {
                return null;
            }

            if (regionName != null)
            {
                throw new NotSupportedException(REGION_NOT_SUPPORTED);
            }

            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            return _store.Get(key);
        }

        private object AddOrGetExistingInternal(string key, object value, CacheItemPolicy policy)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            DateTimeOffset absExp = InfiniteAbsoluteExpiration;
            TimeSpan slidingExp = NoSlidingExpiration;
            CacheItemPriority priority = CacheItemPriority.Default;

            if (policy != null)
            {
                ValidatePolicy(policy);
                absExp = policy.AbsoluteExpiration;
                slidingExp = policy.SlidingExpiration;
                priority = policy.Priority;
            }

            return _store.Set(new RedisCacheEntry(key, value, absExp, slidingExp, priority));
        }

        private void ValidatePolicy(CacheItemPolicy policy)
        {
            if (policy.RemovedCallback != null)
            {
                throw new NotSupportedException("Removed Callback is not supported at this time");
            }

            if (policy.UpdateCallback != null)
            {
                throw new NotSupportedException("Update Callback is not supported at this time");
            }

            if (policy.ChangeMonitors.Count != 0)
            {
                throw new NotSupportedException("Change Monitors are not supported at this time");
            }

            if (policy.AbsoluteExpiration != InfiniteAbsoluteExpiration
                && policy.SlidingExpiration != NoSlidingExpiration)
            {
                throw new ArgumentException("Invalid expiration combination", "policy");
            }

            if (policy.SlidingExpiration < NoSlidingExpiration || OneYear < policy.SlidingExpiration)
            {
                throw new ArgumentOutOfRangeException("policy");
            }

            if (policy.Priority != CacheItemPriority.Default && policy.Priority != CacheItemPriority.NotRemovable)
            {
                throw new ArgumentOutOfRangeException("policy");
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {

            }
        }
    }
}
