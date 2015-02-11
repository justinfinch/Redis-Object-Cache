using System;
using System.Configuration;

namespace RedisObjectCache
{
    // Use the following web.config file.
    //<?xml version="1.0" encoding="utf-8" ?>
    //<configuration>
    //  <configSections>
    //    <section name="redisCache" type="RedisObjectCache.RedisCacheConfiguration" />
    //  </configSections>
    //  <redisCache>
    //    <connection host = "127.0.0.1" [String]
    //          port = "" [number]
    //          accessKey = "" [String]
    //          ssl = "false" [true|false]
    //          databaseId = "0" [number]
    //          applicationName = "" [String]
    //          connectionTimeoutInMilliseconds = "5000" [number]
    //          operationTimeoutInMilliseconds = "5000" [number] />
    //  </redisCache>
    //</configuration>
    internal sealed class RedisCacheConfiguration : ConfigurationSection
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

    internal class RedisConnectionConfiguration : ConfigurationElement
    {
        [ConfigurationProperty("host", DefaultValue = "127.0.0.1", IsRequired = true)]
        public String Host
        {
            get
            {
                return (String)this["host"];
            }
            set
            {
                this["host"] = value;
            }
        }

        [ConfigurationProperty("port", DefaultValue = 6379 ,IsRequired = false)]
        public int Port
        {
            get
            {
                return (int)this["port"];
            }
            set
            {
                this["port"] = value;
            }
        }

        [ConfigurationProperty("accessKey", DefaultValue = "", IsRequired = false)]
        public String AccessKey
        {
            get
            {
                return (String)this["accessKey"];
            }
            set
            {
                this["accessKey"] = value;
            }
        }

        [ConfigurationProperty("ssl", DefaultValue = false, IsRequired = false)]
        public bool Ssl
        {
            get
            {
                return (bool)this["ssl"];
            }
            set
            {
                this["ssl"] = value;
            }
        }

        [ConfigurationProperty("databaseId", DefaultValue = 0, IsRequired = false)]
        public int DatabaseId
        {
            get
            {
                return (int)this["databaseId"];
            }
            set
            {
                this["databaseId"] = value;
            }
        }

        [ConfigurationProperty("connectionTimeoutInMilliseconds", DefaultValue = 5000, IsRequired = false)]
        public int ConnectionTimeoutInMilliseconds
        {
            get
            {
                return (int)this["connectionTimeoutInMilliseconds"];
            }
            set
            {
                this["connectionTimeoutInMilliseconds"] = value;
            }
        }

        [ConfigurationProperty("operationTimeoutInMilliseconds", DefaultValue = 5000, IsRequired = false)]
        public int OperationTimeoutInMilliseconds
        {
            get
            {
                return (int)this["operationTimeoutInMilliseconds"];
            }
            set
            {
                this["operationTimeoutInMilliseconds"] = value;
            }
        }
    }
}
