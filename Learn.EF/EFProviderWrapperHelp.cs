using EFCachingProvider;
using EFCachingProvider.Caching;
using EFTracingProvider;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.EF
{
    public class EFProviderWrapperHelp
    {
        private static ILog logger;
        public void Init()
        {
            EFTracingProviderConfiguration.RegisterProvider();
            EFCachingProviderConfiguration.RegisterProvider();
#if LOGGING
            EFTracingProviderConfiguration.LogToConsole = true; //Optional: for sending tracing to console
#endif
            // 初始化Log4net,配置在独立的"log4net.config"中配置
            log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));
            // 初始化一个logger
            logger = log4net.LogManager.GetLogger("EFLog4net");
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
                using (var context = new ExtendedDataBase())
                {
                    // set up caching
                    context.Cache = cache;
                    context.CachingPolicy = cachingPolicy;

                    //cust.Orders.Load();
                    //context.SaveChanges();
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


        private static void SimpleTracingDemo()
        {
            // disable global logging to console
            EFTracingProviderConfiguration.LogToConsole = false;

                using (var context = new ExtendedDataBase())
                {
                    context.Logger = logger;

                    // this will produce LIKE 'ALFKI%' T-SQL
                    
                    //customer.Orders.Load();

                    context.CommandExecuting += (sender, e) =>
                    {
                        Console.WriteLine("Command is executing: {0}", e.ToTraceString());
                    };

                    context.CommandFinished += (sender, e) =>
                    {
                        Console.WriteLine("Command has finished: {0}", e.ToTraceString());
                    };

                    //context.AddToCustomers(newCustomer);
                    //context.SaveChanges();

                    //context.DeleteObject(newCustomer);

                    //context.SaveChanges();
                }
            

            Console.WriteLine("LOG FILE CONTENTS:");
            Console.WriteLine(File.ReadAllText("sqllogfile.txt"));
        }
    }
}
