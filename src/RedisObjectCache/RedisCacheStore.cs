using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;

namespace RedisObjectCache
{
    internal sealed class RedisCacheStore
    {
        private readonly IDatabase _redisDatabase;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        internal RedisCacheStore(IDatabase redisDatabase)
        {
            _redisDatabase = redisDatabase;

            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            };
        }

        internal object Set(RedisCacheEntry entry)
        {
            var entryJson = JsonConvert.SerializeObject(entry, _jsonSerializerSettings);
            _redisDatabase.StringSet(entry.Key, entryJson, entry.State.GetTtl());

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
            var entryJson = _redisDatabase.StringGet(key);

            if (string.IsNullOrEmpty(entryJson))
                return null;

            var entry = DeserializeRedisCacheEntry(entryJson);

            if (entry.State.IsSliding)
            {
                _redisDatabase.KeyExpire(key, entry.State.GetTtl());
            }

            var itemValue = entry.ItemValue;

            return itemValue;
        }

        internal object Remove(string key)
        {
            var entryJson = _redisDatabase.StringGet(key);
            if (string.IsNullOrEmpty(entryJson))
                return null;
            _redisDatabase.KeyDelete(key);

            var entry = DeserializeRedisCacheEntry(entryJson);
            var itemValue = entry.ItemValue;

            return itemValue;
        }
    }
}
