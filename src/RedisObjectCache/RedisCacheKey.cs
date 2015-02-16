using System;

namespace RedisObjectCache
{
    internal class RedisCacheKey
    {
        internal int Hash { get; set; }
        internal string Key { get; set; }
        internal string StateKey { get; set; }

        internal RedisCacheKey(String key)
        {    
            Key = key;
            StateKey = String.Format("{0}_STATE", Key);
            Hash = key.GetHashCode();
        }    
    }
}
