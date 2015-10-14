
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.TextTemplating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Learn.T4
{
public class TransformContext
{
    public static TransformContext Current { get; internal set; }
    public TextTransformation Transformation{get; private set;}
    public ITextTemplatingEngineHost Host {get; private set;}
    public DTE Dte { get; private set; }
    public ProjectItem TemplageProjectItem { get; private set; }

    internal TransformContext(TextTransformation transformation, ITextTemplatingEngineHost host)
    {           
        this.Transformation = transformation;
        this.Host = host;
        this.Dte = (DTE)((IServiceProvider)host).GetService(typeof(DTE));
        this.TemplageProjectItem = this.Dte.Solution.FindProjectItem(host.TemplateFile);
    }

    public static void EnsureContextInitialized()
    {
        if (null == Current)
        {
            throw new TransformationException("TransformContext is not initialized.");
        }
    }
}
}
