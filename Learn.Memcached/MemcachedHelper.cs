using Enyim.Caching;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Learn.Memcached
{

    [Serializable]
    public class Person
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
    }  

    public class MemcachedHelper
    {
        private static MemcachedClient client = new MemcachedClient("enyim.com/memcached");

        public void SetDemo()
        {
            Person person = new Person { UserId = 1, UserName = "李刚" };
            //不带过期时间的存储，Memcached将根据LRU来决定过期策略  
            bool success = client.Store(StoreMode.Add, person.UserName, person);
            Console.WriteLine("存储[{0}]的结果：{1}", person.UserName, success);
            //带过期时间的缓存  
            success = client.Store(StoreMode.Add, person.UserName, person, DateTime.Now.AddMinutes(10));  
            Console.WriteLine("存储[{0}]的结果：{1}", person.UserName, success);
        }

        public void GetDemo()
        {
            Person person = client.Get<Person>("李刚");
            if (person != null)
            {
                Console.WriteLine("取回[{0}]的结果——UserId:{1},UserName:{2}", "李刚", person.UserId, person.UserName);
            }
            else
            {
                Console.WriteLine("取回[{0}]失败!", "李刚");
            }
        }

        public void MultiGetDemo()
        {
            List<string> personNameList = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                personNameList.Add("李刚00" + i);
            }
            //批量获取，只通过一次网络通讯就取回所有personNameList中的指定的所有数据  
            IDictionary<string, object> resultList = client.Get(personNameList);
            Person person;
            foreach (KeyValuePair<string, object> item in resultList)
            {
                person = item.Value as Person;
                if (person != null)
                {
                    Console.WriteLine("取回[{0}]的结果——UserId:{1},UserName:{2}", "李刚", person.UserId, person.UserName);
                }
                else
                {
                    Console.WriteLine("取回[{0}]失败!", "李刚");
                }
            }
        }

   

    }
}
