using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.T4
{


    public class DemoGenerator : Generator
    {
        public DemoGenerator(string folder)
        {
            this.folder = folder;
        }
        private string folder;

        protected override IDictionary<string, Template> CreateTemplates()
        {
            Dictionary<string, Template> templates = new Dictionary<string, Template>();
            templates.Add("Foo.cs", new DemoTemplate("Foo", folder));
            templates.Add("Bar.cs", new DemoTemplate("Bar", folder));
            templates.Add("Baz.cs", new DemoTemplate("Baz", folder));
            return templates;
        }
    }
}
