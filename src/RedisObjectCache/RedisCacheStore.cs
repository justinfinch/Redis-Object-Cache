using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;

namespace RedisObjectCache
{
    public sealed class RedisCacheStore
    {
        private readonly IConnectionMultiplexer _connection;
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public RedisCacheStore(IConnectionMultiplexer connection)
        {
            _connection = connection;
        }

        public RedisCacheStore()
        {
            var connectionSettings = RedisCacheConfiguration.Instance.Connection;

            var configurationOptions = new ConfigurationOptions
            {
                EndPoints = { { connectionSettings.Host, connectionSettings.Port } },
                ConnectTimeout = connectionSettings.ConnectionTimeoutInMilliseconds,
                Password = connectionSettings.AccessKey,
                Ssl = connectionSettings.Ssl,
                SyncTimeout = connectionSettings.OperationTimeoutInMilliseconds

            };

            _connection = ConnectionMultiplexer.Connect(configurationOptions);
        }

        internal object Set(RedisCacheEntry entry)
        {
            var database = _connection.GetDatabase();
            var entryJson = JsonConvert.SerializeObject(entry, _jsonSerializerSettings);
            database.StringSet(entry.Key, entryJson, entry.State.GetTtl());

            var itemValue = entry.ItemValue;

            return itemValue;
        }

        private RedisCacheEntry DeserializeRedisCacheEntry(string entryJson)
        {
            var entryJsonParsed = JObject.Parse(entryJson);

            var key = entryJsonParsed["Key"].ToString();
            var itemType = entryJsonParsed["ItemType"].ToString();
            var state = JsonConvert.DeserializeObject<RedisCacheEntryState>(entryJsonParsed["State"].ToString());
            var itemValue = JsonConvert.DeserializeObject(entryJsonParsed["ItemValue"].ToString(), Type.GetType(itemType));

            var redisCacheEntry = new RedisCacheEntry(key, itemValue, state.AbsoluteExpiration, state.SlidingExpiration, state.Priority);

            return redisCacheEntry;
        }

        internal object Get(string key)
        {
            var database = _connection.GetDatabase();
            var entryJson = database.StringGet(key);

            if (string.IsNullOrEmpty(entryJson))
                return null;

            var entry = DeserializeRedisCacheEntry(entryJson);

            if (entry.State.IsSliding)
            {
                database.KeyExpire(key, entry.State.GetTtl());
            }

            var itemValue = entry.ItemValue;

            return itemValue;
        }

        internal object Remove(string key)
        {
            var database = _connection.GetDatabase();
            var entryJson = database.StringGet(key);
            if (string.IsNullOrEmpty(entryJson))
                return null;
            database.KeyDelete(key);

            var entry = DeserializeRedisCacheEntry(entryJson);
            var itemValue = entry.ItemValue;

            return itemValue;
        }
    }
}
