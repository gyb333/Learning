using CacheCow.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Learn.CacheCow
{
    class CacheCowHelper
    {
        void init(HttpConfiguration config)
        {
            var cacheCowCacheHandler = new CachingHandler(config);
            config.MessageHandlers.Add(cacheCowCacheHandler);


            //客户端


            HttpClient client = new HttpClient(new WebRequestHandler()
            {
                CachePolicy = new RequestCachePolicy(RequestCacheLevel.Default)
            });

            //var httpResponseMessage = await client.GetAsync("http://superpoopy");

        }
    }
}
