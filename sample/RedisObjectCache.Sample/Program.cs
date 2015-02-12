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

            RedisCache.Default.Add("poco", person1, new CacheItemPolicy() { AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddSeconds(30)) });
            RedisCache.Default.Add("int", 123, new CacheItemPolicy() { AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddSeconds(30)) });
            RedisCache.Default.Add("string", "hello world", new CacheItemPolicy() { AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddSeconds(30)) });
            //RedisCache.Default.Add("test_person_1", person1, new CacheItemPolicy(){ SlidingExpiration = new TimeSpan(0, 0, 0, 30)});

            OutputItem();

        }

        private static void OutputItem()
        {
            Console.WriteLine("====>POCO Object");
            var person = (Person) RedisCache.Default["poco"];;
            Console.WriteLine(person.FirstName);
            Console.WriteLine(person.LastName);

            Console.WriteLine("====>INT");
            Console.WriteLine((int)RedisCache.Default["int"]);

            Console.WriteLine("====>String");
            Console.WriteLine(RedisCache.Default["string"]);

            Console.ReadLine();

            OutputItem();
        }
    }
}
