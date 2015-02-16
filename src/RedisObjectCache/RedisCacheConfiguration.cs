using System.Configuration;

namespace RedisObjectCache
{
    // Use the following web.config file.
    //<?xml version="1.0" encoding="utf-8" ?>
    //<configuration>
    //  <configSections>
    //    <section name="redisCache" type="RedisObjectCache.RedisCacheConfiguration, RedisObjectCache" />
    //  </configSections>
    //  <redisCache>
    //    <connection host = "127.0.0.1" [String]
    //          port = "" [number]
    //          accessKey = "" [String]
    //          ssl = "false" [true|false]
    //          databaseId = "0" [number]
    //          connectionTimeoutInMilliseconds = "5000" [number]
    //          operationTimeoutInMilliseconds = "5000" [number] />
    //  </redisCache>
    //</configuration>
    public sealed class RedisCacheConfiguration : ConfigurationSection
    {
        // Properties
        public static RedisCacheConfiguration Instance
        {
            get { return (RedisCacheConfiguration)ConfigurationManager.GetSection("redisCache"); }
        }

        [ConfigurationProperty("connection", IsRequired = true)]
        public RedisConnectionConfiguration Connection
        {
            get { return (RedisConnectionConfiguration)this["connection"]; }
        }
    }
}
