using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
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
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        }

        internal object Set(RedisCacheEntry entry)
        {
            var ttl = GetTtl(entry.State);

            var valueJson = JsonConvert.SerializeObject(entry.Value, _jsonSerializerSettings);
            var stateJson = JsonConvert.SerializeObject(entry.State, _jsonSerializerSettings);

            _redisDatabase.StringSet(entry.Key, valueJson, ttl);
            _redisDatabase.StringSet(entry.StateKey, stateJson, ttl);

            return entry.Value;
        }

        internal object Get(string key)
        {
            var redisCacheKey = new RedisCacheKey(key);

            var stateJson = _redisDatabase.StringGet(redisCacheKey.StateKey);
            if (string.IsNullOrEmpty(stateJson))
                return null;

            var valueJson = _redisDatabase.StringGet(redisCacheKey.Key);
            var state = JsonConvert.DeserializeObject<RedisCacheEntryState>(stateJson);

            var value = GetObjectFromString(valueJson, state.TypeName);

            if (state.IsSliding)
            {
                state.UpdateUsage();
                stateJson = JsonConvert.SerializeObject(state, _jsonSerializerSettings);

                var ttl = GetTtl(state);
                _redisDatabase.StringSet(redisCacheKey.StateKey, stateJson, ttl);
                _redisDatabase.KeyExpire(redisCacheKey.Key, ttl);
            }

            return value;
        }

        internal object Remove(string key)
        {
            var redisCacheKey = new RedisCacheKey(key);
            var valueJson = _redisDatabase.StringGet(redisCacheKey.Key);
            if (string.IsNullOrEmpty(valueJson))
                return null;

            var value = JsonConvert.DeserializeObject(valueJson);

            _redisDatabase.KeyDelete(redisCacheKey.Key);
            _redisDatabase.KeyDelete(redisCacheKey.StateKey);

            return value;
        }

        private TimeSpan GetTtl(RedisCacheEntryState state)
        {
            return state.UtcAbsoluteExpiration.Subtract(DateTime.UtcNow);
        }

        private object GetObjectFromString(string json, string typeName)
        {
            MethodInfo method = typeof(JsonConvert).GetMethods().FirstOrDefault(m => m.Name == "DeserializeObject" && m.IsGenericMethod);
            var t = Type.GetType(typeName);
            MethodInfo genericMethod = method.MakeGenericMethod(t);
            return genericMethod.Invoke(null, new object[]{ json }); // No target, no arguments
        }
    }
}
