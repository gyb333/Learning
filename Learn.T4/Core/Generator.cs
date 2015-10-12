using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.T4
{
public abstract class Generator
{
    private string directory;

    protected virtual IDictionary<string, Template> CreateTemplates()
    {
        return new Dictionary<string, Template>();
    }
    protected virtual Template CreateTemplate()
    {
        return null;
    }
    public void Run()
    {
        
        this.RunCore();
        //this.RemoveUnusedFiles();
    }
    protected virtual void RunCore()
    {
        List<string> files = new List<string>();
        directory = Path.GetDirectoryName(TransformContext.Current.Host.TemplateFile);
        foreach (var item in this.CreateTemplates())
        {
    

            files.Add(Path.Combine(directory,item.Key));
            
            item.Value.EnsureInitialized();
            item.Value.RenderToFile(item.Key);
        }

        Template template = this.CreateTemplate();
        if (null != template)
        {
            template.EnsureInitialized();
            template.Render();
        }   
    }
    protected virtual void RemoveUnusedFiles()
    {
        List<string> files = new List<string>();
        directory = Path.GetDirectoryName(TransformContext.Current.Host.TemplateFile);
        foreach (var item in this.CreateTemplates())
        {
            files.Add(Path.Combine(directory, item.Key));
            item.Value.EnsureInitialized();
            item.Value.RenderToFile(item.Key);
        }
        var projectItems = TransformContext.Current.TemplageProjectItem.ProjectItems.Cast<ProjectItem>().ToArray();
        foreach (ProjectItem projectItem in projectItems)
        {
            string fileName = projectItem.get_FileNames(0);
            if (!files.Contains(fileName))
            {
                projectItem.Delete();
            }
        }
    }
}
}
