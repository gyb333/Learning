// <copyright file="VolatileAssemblyProcessor.cs" company="T4 Toolbox Team">
//  Copyright © T4 Toolbox Team. All Rights Reserved.
// </copyright>

namespace T4Toolbox
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using EnvDTE;
    using Microsoft.VisualStudio.TextTemplating;

    /// <summary>
    /// Processes VolatileAssembly custom directive.
    /// </summary>
    /// <remarks>
    /// VolatileAssembly custom directive works similar to the built-in Assembly directive,
    /// but does not lock the referenced assembly. Use this directive when you need to 
    /// reference a custom assembly in your code generator that has to be recompiled 
    /// between template transformations.
    /// </remarks>
    public class VolatileAssemblyProcessor : DirectiveProcessor
    {
        /// <summary>
        /// Name parameter name.
        /// </summary>
        internal const string NameParameterName = "name";

        /// <summary>
        /// Flag indicating if the host has been forced to unload the generation AppDomain.
        /// </summary>
        private bool appDomainUnloaded;

        /// <summary>
        /// Gets the directive name.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> that contains name of the VolatileAssembly directive.
        /// </value>
        protected override string DirectiveName
        {
            get { return "VolatileAssembly"; }
        }

        /// <summary>
        /// T4 <see cref="Microsoft.VisualStudio.TextTemplating.Engine"/> calls this 
        /// method in the beginning of template transformation.
        /// </summary>
        /// <param name="host">
        /// The <see cref="ITextTemplatingEngineHost"/> object hosting the transformation.
        /// </param>
        /// <remarks>
        /// When this method is called by the host, this DirectiveProcessor cleans up the temp
        /// directory that is used to store the copies of the assemblies. Old unused copies are
        /// removed.
        /// </remarks>
        public override void Initialize(ITextTemplatingEngineHost host)
        {
            base.Initialize(host);
            CleanTempDirectory();
        }

        /// <summary>
        /// Begins a round of directive processing.
        /// </summary>
        /// <param name="languageProvider">CodeDom language provider for generating code.</param>
        /// <param name="templateContents">Contents of the T4 template.</param>
        /// <param name="errors">Compiler Errors.</param>
        public override void StartProcessingRun(CodeDomProvider languageProvider, string templateContents, CompilerErrorCollection errors)
        {
            this.appDomainUnloaded = false;
            base.StartProcessingRun(languageProvider, templateContents, errors);
        }

        /// <summary>
        /// Processes an instance of the VolatileAssembly directive.
        /// </summary>
        /// <param name="directiveName">The name of the directive processor.</param>
        /// <param name="arguments">Arguments for the directive.</param>
        /// <remarks>
        /// This method copies the referenced assembly into the temp directory (if a current copy does not
        /// already exist), and then adds this temp copy to the list of references.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Arguments are validated in base method")]
        public override void ProcessDirective(string directiveName, IDictionary<string, string> arguments)
        {
            base.ProcessDirective(directiveName, arguments);
            try
            {
                if (!arguments.ContainsKey(NameParameterName))
                {
                    DirectiveProcessorException e = new DirectiveProcessorException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "{0} is a required parameter for the {1} directive",
                            NameParameterName,
                            this.DirectiveName));
                    e.Source = Host.TemplateFile;
                    throw e;
                }

                this.References.Add(this.GetTempAssemblyName(arguments[NameParameterName]));
            }
            catch (DirectiveProcessorException e)
            {
                // Report expected errors without the call stack
                CompilerError error = new CompilerError();
                error.ErrorText = e.Message;
                error.FileName = e.Source;
                this.Errors.Add(error);
            }
        }

        /// <summary>
        /// Gets the temporary directory where copies of the assembly are stored.
        /// </summary>
        /// <returns>Temp directory path.</returns>
        private static string GetTempPath()
        {
            string path = Path.Combine(Path.GetTempPath(), "VolatileAssemlyProcessor");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        /// <summary>
        /// Gets the latest temp copy of the referenced assembly. If none exists, String.Empty is returned.
        /// </summary>
        /// <param name="assemblyName">Referenced assembly name.</param>
        /// <returns>Path of latest tempy copy of assembly. String.Empty if none found.</returns>
        private static string GetLatestCopy(string assemblyName)
        {
            DateTime lastModified = DateTime.MinValue;
            string lastModifiedFile = string.Empty;
            foreach (var file in Directory.GetFiles(GetTempPath(), Path.GetFileNameWithoutExtension(assemblyName) + "*"))
            {
                DateTime currentFileLastModified;
                if ((currentFileLastModified = File.GetLastWriteTimeUtc(file)) > lastModified)
                {
                    lastModified = currentFileLastModified;
                    lastModifiedFile = file;
                }
            }

            return lastModifiedFile;
        }

        /// <summary>
        /// Deletes old and unused temp copies of referenced assemblies. 
        /// </summary>
        private static void CleanTempDirectory()
        {
            foreach (var file in Directory.GetFiles(GetTempPath()))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    if (IsCritical(e))
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the exception is a critical exception.
        /// </summary>
        /// <param name="ex">Exception thrown.</param>
        /// <returns>True if the exception is critical.</returns>
        private static bool IsCritical(Exception ex)
        {
            return (ex is OutOfMemoryException)
                || (ex is AppDomainUnloadedException)
                || (ex is BadImageFormatException)
                || (ex is CannotUnloadAppDomainException)
#if !DEV10
                || (ex is ExecutionEngineException)
#endif
                || (ex is InvalidProgramException)
                || (ex is ThreadAbortException);
        }

        /// <summary>
        /// Forces the host to unload the generation AppDomain so that new versions of the generated assembly can be loaded.
        /// </summary>
        private void UnloadGenerationAppDomain()
        {
            // HACK!HACK!: Reflection hack for now to force the unloading of the code-gen AppDomain every time we need to load a new version
            // of the assembly. This assumes that the host is the Visual Studio host.
            if (!this.appDomainUnloaded)
            {
                MethodInfo method = Host.GetType().GetMethod("UnloadGenerationAppDomain", BindingFlags.Instance | BindingFlags.NonPublic);
                if (method != null)
                {
                    method.Invoke(Host, new object[] { });
                    this.appDomainUnloaded = true;
                }
            }
        }

        /// <summary>
        /// Resolves assembly reference to an absolute path name.
        /// </summary>
        /// <param name="assemblyName">An assembly reference.</param>
        /// <returns>An absolute path to the assembly.</returns>
        private string ResolveAssemblyReference(string assemblyName)
        {
            string assemblyPath = assemblyName;

            // Try to resolve assembly name as relative path first
            try
            {
                assemblyPath = Host.ResolvePath(assemblyName);
                if (File.Exists(assemblyPath))
                {
                    return assemblyPath;
                }
            }
            catch (FileNotFoundException)
            {
            }

            // Try to resolve assembly name
            try
            {
                assemblyPath = Host.ResolveAssemblyReference(assemblyName);
                if (File.Exists(assemblyPath))
                {
                    return assemblyPath;
                }
            }
            catch (FileNotFoundException) 
            { 
            }
            catch (FileLoadException) 
            { 
            }
                
            DirectiveProcessorException directiveProcessorException = new DirectiveProcessorException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    "{0} directive: Assembly '{1}' resolved as '{2}' could not be found.",
                    this.DirectiveName,
                    assemblyName,
                    assemblyPath));
            directiveProcessorException.Source = Host.TemplateFile;
            throw directiveProcessorException;
        }

        /// <summary>
        /// Gets the name of the temp copy of the referenced assembly. If a current copy does not exist, a new copy is created.
        /// </summary>
        /// <param name="assemblyName">Referenced assembly name.</param>
        /// <returns>Path of temp copy of assembly.</returns>
        private string GetTempAssemblyName(string assemblyName)
        {
            string assemblyPath = this.ResolveAssemblyReference(assemblyName);
            string existingCopy = GetLatestCopy(assemblyName);
            if (String.IsNullOrEmpty(existingCopy) || File.GetLastWriteTimeUtc(existingCopy) < File.GetLastWriteTimeUtc(assemblyPath))
            {
                return this.CreateNewCopy(assemblyPath);
            }

            return existingCopy;
        }

        /// <summary>
        /// Creates a new temp copy of the referenced assembly.
        /// </summary>
        /// <param name="assemblyPath">Referenced assembly name.</param>
        /// <returns>Path of newly created temp copy of assembly.</returns>
        private string CreateNewCopy(string assemblyPath)
        {
            string copyPath = Path.Combine(GetTempPath(), Path.GetFileNameWithoutExtension(assemblyPath) + Guid.NewGuid().ToString() + Path.GetExtension(assemblyPath));
            File.Copy(assemblyPath, copyPath);
            this.UnloadGenerationAppDomain();
            return copyPath;
        }
    }
}
