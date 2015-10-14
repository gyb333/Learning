using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.T4 
{
    public class DemoTemplate : Template
    {
        public string ClassName { get; private set; }
        public DemoTemplate(string className)
        {
            this.ClassName = className;
        }
        public override string TransformText()
        {
            this.WriteLine("public class {0}", this.ClassName);
            this.WriteLine("{");
            this.WriteLine("}");
            return this.GenerationEnvironment.ToString();
        }
    }
}
