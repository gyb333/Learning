using EFCachingProvider;
using EFCachingProvider.Caching;
using EFTracingProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.EF
{
    public class EFProviderWrapperHelp
    {
        public void Init()
        {
            EFTracingProviderConfiguration.RegisterProvider();
            EFCachingProviderConfiguration.RegisterProvider();
#if LOGGING
            EFTracingProviderConfiguration.LogToConsole = true; //Optional: for sending tracing to console
#endif

        }


        private static void SimpleCachingDemo()
        {
            ICache cache = new InMemoryCache();
            CachingPolicy cachingPolicy = CachingPolicy.CacheAll;

            // log SQL from all connections to the console
            EFTracingProviderConfiguration.LogToConsole = true;

            for (int i = 0; i < 3; ++i)
            {
                Console.WriteLine();
                Console.WriteLine("*** Pass #{0}...", i);
                Console.WriteLine();
                using (var context = new ExtendedNorthwindEntities())
                {
                    // set up caching
                    context.Cache = cache;
                    context.CachingPolicy = cachingPolicy;

                    Console.WriteLine("Loading customer...");
                    var cust = context.Customers.First(c => c.CustomerID == "ALFKI");
                    Console.WriteLine("Customer name: {0}", cust.ContactName);
                    Console.WriteLine("Loading orders...");
                    cust.Orders.Load();
                    Console.WriteLine("Order count: {0}", cust.Orders.Count);
                }
            }

            Console.WriteLine();

            //Console.WriteLine("*** Cache statistics: Hits:{0} Misses:{1} Hit ratio:{2}% Adds:{3} Invalidations:{4}",
            //    cache.CacheHits,
            //    cache.CacheMisses,
            //    100.0 * cache.CacheHits / (cache.CacheHits + cache.CacheMisses),
            //    cache.CacheItemAdds,
            //    cache.CacheItemInvalidations);
        }
    }
}
