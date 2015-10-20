using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.structuremap
{
    public interface IBar
    {
    }

    public class Bar : IBar
    {
    }

    public interface IFoo
    {
    }

    public class Foo : IFoo
    {
        public IBar Bar { get; private set; }

        public Foo(IBar bar)
        {
            Bar = bar;
        }
    }
}
