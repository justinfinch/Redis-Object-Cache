using System;
using System.Runtime.Caching;

namespace RedisObjectCache.Sample
{
    public class Program
    {
        static void Main(string[] args)
        {
            var person1 = new Person()
            {
                FirstName = "Bob",
                LastName = "Smith"
            };

            const int secondsInCache = 10;

            RedisCache.Default.Add("absolutePoco", person1, new CacheItemPolicy() { AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddSeconds(secondsInCache)) });
            RedisCache.Default.Add("int", 123, new CacheItemPolicy() { AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddSeconds(secondsInCache)) });
            RedisCache.Default.Add("string", "hello world", new CacheItemPolicy() { AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddSeconds(secondsInCache)) });
            RedisCache.Default.Add("slidingPoco", person1, new CacheItemPolicy() { SlidingExpiration = new TimeSpan(0, 0, 0, secondsInCache) });

            OutputItem();
        }

        private static void OutputItem()
        {
            Console.WriteLine("====>POCO Object");
            var absolutePoco = RedisCache.Default["absolutePoco"] as Person;
            if (absolutePoco != null)
            {
                Console.WriteLine(absolutePoco.FirstName);
                Console.WriteLine(absolutePoco.LastName);
            }
            else
            {
                Console.WriteLine("Not in cache");
            }

            Console.WriteLine("====>INT");
            var intValue = RedisCache.Default["int"] as int?;
            if (intValue.HasValue)
            {
                Console.WriteLine(intValue);
            }
            else
            {
                Console.WriteLine("Not in cache");
            }

            Console.WriteLine("====>String");
            var stringValue = RedisCache.Default["int"] as string;
            if (!string.IsNullOrEmpty(stringValue))
            {
                Console.WriteLine(RedisCache.Default["stringValue"]);
            }
            else
            {
                Console.WriteLine("Not in cache");
            }

            Console.WriteLine("====>Sliding POCO Object");
            var slidingPoco = RedisCache.Default["slidingPoco"] as Person;
            if (slidingPoco != null)
            {
                Console.WriteLine(slidingPoco.FirstName);
                Console.WriteLine(slidingPoco.LastName);
            }
            else
            {
                Console.WriteLine("Not in cache");
            }

            Console.ReadLine();

            OutputItem();
        }
    }
}
