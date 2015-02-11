using System;
using System.Runtime.Caching;
using Newtonsoft.Json;

namespace RedisObjectCache.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var person1 = new Person()
            {
                FirstName = "Bob",
                LastName = "Smith"
            };

            RedisCache.Default.Add("test_person_1", person1, new CacheItemPolicy(){ AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddSeconds(30))});
            //RedisCache.Default.Add("test_person_1", person1, new CacheItemPolicy(){ SlidingExpiration = new TimeSpan(0, 0, 0, 30)});

            OutputItem();

        }

        private static void OutputItem()
        {
            var value = RedisCache.Default["test_person_1"];
            var valueJson = JsonConvert.SerializeObject(value);
            Console.Write(valueJson);

            Console.ReadLine();

            OutputItem();
        }
    }
}
