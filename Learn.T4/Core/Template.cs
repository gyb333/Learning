using EnvDTE;
using Microsoft.VisualStudio.TextTemplating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Learn.T4
{
public abstract class Template: TextTransformation
{
    //    <# System.Diagnostics.Debugger.Launch();#>                
    //<#System.Diagnostics.Debugger.Break();#> 
    public Template(string folder)
    {
        this.folder = folder;
    }

    private string folder;


    private bool initialized;
    public override void Initialize()
    {
        base.Initialize();
        initialized = true;
    }
    internal void EnsureInitialized()
    {
        if (!initialized)
        {
            this.Initialize();
        }
    }
    public virtual void Render()
    { 
        TransformContext.EnsureContextInitialized();
        string contents = this.TransformText();
        TransformContext.Current.Transformation.Write(contents);
    }
    public virtual void RenderToFile(string fileName)
    {
            TransformContext.EnsureContextInitialized();
            string directory = Path.GetDirectoryName(TransformContext.Current.Host.TemplateFile);
            directory = Path.Combine(directory, folder);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            fileName = Path.Combine(directory, fileName);
            string contents = this.TransformText();
            this.CreateFile(fileName, contents);
            if (TransformContext.Current.TemplageProjectItem.ProjectItems.Cast<ProjectItem>().Any(item => item.get_FileNames(0) != fileName))
            {
                TransformContext.Current.TemplageProjectItem.ProjectItems.AddFromFile(fileName);
                 
            }
    }
    protected void CreateFile(string fileName, string contents)
    {
        if (File.Exists(fileName) && File.ReadAllText(fileName) == contents)
        {
            return;
        }
        SourceControl sourceControl = TransformContext.Current.Dte.SourceControl;
        if (null != sourceControl && sourceControl.IsItemUnderSCC(fileName) && !sourceControl.IsItemCheckedOut(fileName))
        {
            sourceControl.CheckOutItem(fileName);
        }
        File.WriteAllText(fileName, contents);
    }
}
}
