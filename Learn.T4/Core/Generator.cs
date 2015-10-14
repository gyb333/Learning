using EnvDTE;
using EnvDTE80;
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
    List<string> files = new List<string>();
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
        this.RemoveUnusedFiles();
    }
    protected virtual void RunCore()
    {
       
        directory = Path.GetDirectoryName(TransformContext.Current.Host.TemplateFile);
        directory = Path.Combine(directory, "OutPut");
        foreach (var item in this.CreateTemplates())
        {
            files.Add(Path.Combine(directory,item.Key));
            
            item.Value.EnsureInitialized();
            item.Value.RenderToFile(item.Key,"OutPut");
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
