using StructureMap;
using StructureMap.Configuration.DSL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.structuremap
{
    public class FooBarRegistry : Registry
    {
        public FooBarRegistry()
        {
            For<IFoo>().Use<Foo>();
            For<IBar>().Use<Bar>();
        }

        void test()
        {
            var container = new Container(c => { c.AddRegistry<FooBarRegistry>(); });


            container.GetInstance<IFoo>();
            //using (var nested = someExistingContainer.GetNestedContainer())
            //{
            //    // pull other objects from the nested container and do work with those services
            //    var service = nested.GetInstance<IService>();
            //    service.DoSomething();
            //}

          

        }




 
    }



    public class FooRegistry : Registry
    {

        public FooRegistry()
        {
            For<IFoo>().AddInstances(x =>
            {
                x.Type<Foo>()
                    .Ctor<string>("url").Is("a url")
                    .Named("Domestic");

                x.Type<Foo>()
                    .Ctor<string>("url").Is("a different url")
                    .Named("International");

                x.Type<Foo>().Named("Internal");
            });
        }


        void test()
        {
            var container = new Container(new FooRegistry());

            // Accessing the IShippingService Instance's by name
            var internationalService = container.GetInstance<IFoo>("International");
            var domesticService = container.GetInstance<IFoo>("Domestic");
            var internalService = container.GetInstance<IFoo>("Internal");



            var container3 = Container.For<FooBarRegistry>();

        }
    }
}
