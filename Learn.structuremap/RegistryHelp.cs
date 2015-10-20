 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Shouldly;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;
using StructureMap;
using System.Data;
using System.Data.SqlClient;
namespace Learn.structuremap
{

    public class RegistryHelp : Registry
    {
        //显式地构建一个StructureMap Container对象
        public void DisplayRegistryContainer()
        {
            var container = new Container(_ =>
            {
                _.For<IFoo>().Use<Foo>();
                _.For<IBar>().Use<Bar>();
            });

            // Now, resolve a new object instance of IFoo
            container.GetInstance<IFoo>()
                // should be type Foo
                .ShouldBeOfType<Foo>()

                // and the IBar dependency too
                .Bar.ShouldBeOfType<Foo>();




        }


        private void RegistryContainer()
        {
            var container = new  Container(_ =>
            {
                _.Scan(x =>
                {
                    x.TheCallingAssembly();
                    x.WithDefaultConventions();
                });
            });

            var container2 = new Container();

            container2.Configure(c =>
                c.Scan(scanner =>
                {
                    scanner.TheCallingAssembly();
                    scanner.WithDefaultConventions();
                }));

            var container3 = new Container(c =>
            {
                //just for demo purposes, normally you don't want to embed the connection string directly into code.
                c.For<IDbConnection>().Use<SqlConnection>().Ctor<string>().Is("YOUR_CONNECTION_STRING");
                //a better way would be providing a delegate that retrieves the value from your app config.    
            });



            container.GetInstance<IFoo>()
                .ShouldBeOfType<Foo>()
                .Bar.ShouldBeOfType<Foo>();
        }


       
    }


    

}
