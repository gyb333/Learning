using Microsoft.VisualStudio.TextTemplating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Learn.T4
{
public static class TextTransformationExtensions
{
    public static void RunCodeGenerator(this TextTransformation transformation, ITextTemplatingEngineHost host, Generator generator)
    {
        using (TransformContextScope contextScope = new TransformContextScope(transformation, host))
        {
            generator.Run();
        }
    }

   
}
}
