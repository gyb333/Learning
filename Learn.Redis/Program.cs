using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.Redis
{
    class Program
    {
        static RedisClient rc = new RedisClient("127.0.0.1", 6379);

        static void Main(string[] args)
        {
            string strKey ="addItemToList";
            List<string> ls = new List<string>() { "one", "two", "three" };
            ls.ForEach(x => rc.AddItemToList(strKey, x));

            ls = rc.GetAllItemsFromList(strKey);
            ls.ForEach(s => Console.WriteLine(strKey+":" + s));
            var rcs = rc.Lists[strKey];
            rcs.Clear();
            rcs.Remove("Two");
           

            Console.ReadKey();

        }
    }
}
