// <copyright file="XsdProcessor.cs" company="T4 Toolbox Team">
//  Copyright © T4 Toolbox Team. All Rights Reserved.
// </copyright>

namespace T4Toolbox
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using Microsoft.VisualStudio.TextTemplating;

    /// <summary>
    /// Imports XML schema into a set of strongly typed .NET class definitions.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Have to use large number of classes from the CodeDom namespace")]
    public class XsdProcessor : DirectiveProcessor
    {
        /// <summary>
        /// File parameter name.
        /// </summary>
        internal const string FileParameterName = "file";

        /// <summary>
        /// Indicates whether XML schema validation or compillation encountered any errors.
        /// </summary>
        private bool schemaValidationErrors;

        /// <summary>
        /// Initializes a new instance of the <see cref="XsdProcessor"/> class.
        /// </summary>
        public XsdProcessor()
        {
        }

        /// <summary>
        /// Gets the directive name as it is supposed to be used in the template code.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> that contains name of the xsd directive.
        /// </value>
        protected override string DirectiveName
        {
            get { return "xsd"; }
        }

        /// <summary>
        /// Gets references to pass to the compiler. 
        /// </summary>
        /// <returns>A list of assembly names required for XML serialization.</returns>
        public override string[] GetReferencesForProcessingRun()
        {
            return new string[] { "System.Xml" };
        }

        /// <summary>
        /// Processes a single directive from a template file.
        /// </summary>
        /// <param name="directiveName">Directive name used in the T4 template.</param>
        /// <param name="arguments">Directive arguments specified in the T4 template.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Arguments are validated in base method")]
        public override void ProcessDirective(string directiveName, IDictionary<string, string> arguments)
        {
            base.ProcessDirective(directiveName, arguments);
            try
            {
                // Extract files directive parameter
                if (!arguments.ContainsKey(FileParameterName))
                {
                    DirectiveProcessorException e = new DirectiveProcessorException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "{0} is a required parameter for the {1} directive", 
                            XsdProcessor.FileParameterName, 
                            this.DirectiveName));
                    e.Source = Host.TemplateFile;
                    throw e;
                }

                this.ProcessDirectiveCore(arguments[FileParameterName]);
            }
            catch (DirectiveProcessorException e)
            {
                CompilerError error = new CompilerError();
                error.ErrorText = e.Message;
                error.FileName = e.Source;
                this.Errors.Add(error);
            }
        }

        #region private

        #region static 

        /// <summary>
        /// Generates static Load method for a .NET class that encapsulates a schema element.
        /// </summary>
        /// <param name="xmlTypeMapping"><see cref="XmlTypeMapping"/> object that describes schema element and its class.</param>
        /// <param name="namespace"><see cref="CodeNamespace"/> object that represents namespace where the .NET class is defined.</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Have to use large number of classes from the CodeDom namespace")]
        private static void GenerateLoadMethod(XmlTypeMapping xmlTypeMapping, CodeNamespace @namespace)
        {
            // partial class ElementName
            CodeTypeDeclaration @class = new CodeTypeDeclaration(xmlTypeMapping.ElementName);
            @class.IsClass = true;
            @class.IsPartial = true;
            @namespace.Types.Add(@class);

            // public static ElementName Load(string filePath)
            CodeMemberMethod method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            method.Name = "Load";
            method.ReturnType = new CodeTypeReference(@class.Name);
            @class.Members.Add(method);

            // filePath parameter
            CodeParameterDeclarationExpression filePath = new CodeParameterDeclarationExpression(typeof(string), "filePath");
            method.Parameters.Add(filePath);

            // if (filePath == null)
            CodeConditionStatement @if = new CodeConditionStatement();
            @if.Condition = new CodeBinaryOperatorExpression(
                new CodeArgumentReferenceExpression(filePath.Name),
                CodeBinaryOperatorType.IdentityEquality,
                new CodePrimitiveExpression(null));
            method.Statements.Add(@if);

            // throw new ArgumentNullException("filePath")
            CodeThrowExceptionStatement @throw = new CodeThrowExceptionStatement();
            @throw.ToThrow = new CodeObjectCreateExpression(
                new CodeTypeReference(typeof(ArgumentNullException)),
                new CodePrimitiveExpression(filePath.Name));
            @if.TrueStatements.Add(@throw);

            // XmlSerializer serializer = new XmlSerializer(typeof(Database));
            CodeVariableDeclarationStatement serializerVariable = new CodeVariableDeclarationStatement();
            serializerVariable.Type = new CodeTypeReference(typeof(XmlSerializer));
            serializerVariable.Name = "serializer";
            serializerVariable.InitExpression = new CodeObjectCreateExpression(
                new CodeTypeReference(typeof(XmlSerializer)),
                new CodeTypeOfExpression(@class.Name));
            method.Statements.Add(serializerVariable);

            // FileStream stream = null;
            CodeVariableDeclarationStatement streamVariable = new CodeVariableDeclarationStatement();
            streamVariable.Type = new CodeTypeReference(typeof(FileStream));
            streamVariable.Name = "stream";
            streamVariable.InitExpression = new CodePrimitiveExpression(null);
            method.Statements.Add(streamVariable);

            // try/finally (no using statement in VB)
            CodeTryCatchFinallyStatement tryFinally = new CodeTryCatchFinallyStatement();
            method.Statements.Add(tryFinally);

            // try

            // stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            CodeAssignStatement streamAssign = new CodeAssignStatement();
            streamAssign.Left = new CodeVariableReferenceExpression(streamVariable.Name);
            streamAssign.Right = new CodeObjectCreateExpression(
                new CodeTypeReference(typeof(FileStream)),
                new CodeArgumentReferenceExpression(filePath.Name),
                new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(FileMode)), FileMode.Open.ToString()),
                new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(FileAccess)), FileAccess.Read.ToString()),
                new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(FileShare)), FileShare.Read.ToString()));
            tryFinally.TryStatements.Add(streamAssign);

            // return (ElementName)serializer.Deserialize(stream);
            CodeMethodInvokeExpression serializerDeserialize = new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(serializerVariable.Name), "Deserialize"),
                new CodeVariableReferenceExpression(streamVariable.Name));
            CodeMethodReturnStatement @return = new CodeMethodReturnStatement();
            @return.Expression = new CodeCastExpression(new CodeTypeReference(@class.Name), serializerDeserialize);
            tryFinally.TryStatements.Add(@return);

            // finally

            // if (stream != null)
            CodeConditionStatement ifStreamNotNull = new CodeConditionStatement();
            ifStreamNotNull.Condition = new CodeBinaryOperatorExpression(
                new CodeVariableReferenceExpression(streamVariable.Name),
                CodeBinaryOperatorType.IdentityInequality,
                new CodePrimitiveExpression(null));
            tryFinally.FinallyStatements.Add(ifStreamNotNull);

            // stream.Dispose();
            CodeMethodInvokeExpression streamDispose = new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    new CodeVariableReferenceExpression(streamVariable.Name),
                    "Dispose"));
            ifStreamNotNull.TrueStatements.Add(streamDispose);
        }

        /// <summary>
        /// Imports XML type definitions as .NET classes.
        /// </summary>
        /// <param name="schema">Compiled XML schema.</param>
        /// <param name="schemaImporter">XML Schema Importer.</param>
        /// <param name="codeExporter">XML Code Exporter.</param>
        /// <param name="namespace">Namespace where .NET classes are generated.</param>
        private static void ImportSchemaAsClasses(XmlSchema schema, XmlSchemaImporter schemaImporter, XmlCodeExporter codeExporter, CodeNamespace @namespace)
        {
            foreach (XmlSchemaElement element in schema.Elements.Values)
            {
                if (!element.IsAbstract)
                {
                    XmlTypeMapping xmlTypeMapping = schemaImporter.ImportTypeMapping(element.QualifiedName);
                    codeExporter.ExportTypeMapping(xmlTypeMapping);

                    GenerateLoadMethod(xmlTypeMapping, @namespace);
                }
            }
        }

        /// <summary>
        /// Canonicalizes file path to ensure relative and absolute file paths pointing 
        /// to the same file are represented by the same string.
        /// </summary>
        /// <param name="path">File name or file path.</param>
        /// <returns>Absolute path converted to lower case.</returns>
        private static string NormalizePath(string path)
        {
            path = Path.GetFullPath(path);
            path = path.ToUpperInvariant();
            return path;
        }

        /// <summary>
        /// Resolves relative file path.
        /// </summary>
        /// <param name="basePath">File path of the main file.</param>
        /// <param name="relativePath">Relative path to referred file.</param>
        /// <returns>Absolute path.</returns>
        private static string ResolvePath(string basePath, string relativePath)
        {
            string baseDirectory = Path.GetDirectoryName(basePath);
            string filePath = NormalizePath(Path.Combine(baseDirectory, relativePath));

            if (!File.Exists(filePath))
            {
                DirectiveProcessorException e = new DirectiveProcessorException(
                    string.Format(CultureInfo.CurrentCulture, "File doesn't exist: {0}", filePath));
                e.Source = basePath;
                throw e;
            }

            return filePath;
        }

        #endregion

        /// <summary>
        /// Compiles collection of XML schemas.
        /// </summary>
        /// <param name="schemas">Collection of XML schemas.</param>
        private void CompileSchemas(XmlSchemas schemas)
        {
            schemas.Compile(this.ValidationErrorHandler, true);
            if (!schemas.IsCompiled)
            {
                CompilerError error = new CompilerError();
                error.FileName = Host.TemplateFile;
                error.ErrorText = "Schema could not be compiled";
                error.IsWarning = true;
                this.Errors.Add(error);
            }
        }

        /// <summary>
        /// Generates CodeDom model of the source code based on XML schema.
        /// </summary>
        /// <param name="schemas">A set of compiled XML schemas.</param>
        /// <returns>CodeDom model of the gererated source code.</returns>
        private CodeCompileUnit GenerateCodeDom(XmlSchemas schemas)
        {
            CodeCompileUnit codeCompileUnit = new CodeCompileUnit();
            CodeNamespace @namespace = new CodeNamespace(string.Empty);
            codeCompileUnit.Namespaces.Add(@namespace);
            CodeGenerationOptions options = CodeGenerationOptions.None;
            XmlCodeExporter codeExporter = new XmlCodeExporter(@namespace, codeCompileUnit, this.LanguageProvider, options, null);
            XmlSchemaImporter schemaImporter = new XmlSchemaImporter(schemas, options, this.LanguageProvider, new ImportContext(new CodeIdentifiers(), false));
            foreach (XmlSchema schema in schemas)
            {
                ImportSchemaAsClasses(schema, schemaImporter, codeExporter, @namespace);
            }

            CodeTypeDeclarationCollection types = @namespace.Types;
            if (types == null || types.Count == 0)
            {
                Console.WriteLine("No Classes Generated");
            }

            CodeGenerator.ValidateIdentifiers(@namespace);

            return codeCompileUnit;
        }

        /// <summary>
        /// Processes XML schema specified using file name.
        /// </summary>
        /// <param name="fileName">File name of the XML schema.</param>
        private void ProcessDirectiveCore(string fileName)
        {
            fileName = Environment.ExpandEnvironmentVariables(fileName);
            fileName = ResolvePath(Host.TemplateFile, fileName);
            XmlSchema schema = this.ReadSchema(fileName);

            XmlSchemas schemas = new XmlSchemas();
            schemas.Add(schema);

            this.ReadExternalSchemas(schemas);

            this.CompileSchemas(schemas);

            CodeCompileUnit codeDom = this.GenerateCodeDom(schemas);

            this.LanguageProvider.GenerateCodeFromCompileUnit(codeDom, this.ClassCode, null);
        }

        /// <summary>
        /// Loads external schemas for each schema in the collection.
        /// </summary>
        /// <param name="schemas">Collection of XML schemas.</param>
        /// <remarks>
        /// External schemas are those schemas referenced by import and include schema elements.
        /// </remarks>
        private void ReadExternalSchemas(XmlSchemas schemas)
        {
            Dictionary<string, XmlSchema> externalSchemas = new Dictionary<string, XmlSchema>();

            foreach (XmlSchema schema in schemas)
            {
                if (schema.TargetNamespace != null && schema.TargetNamespace.Length == 0)
                {
                    schema.TargetNamespace = null;
                }

                this.ReadExternalSchemas(schema, externalSchemas, schema);
            }

            foreach (XmlSchema schema in externalSchemas.Values)
            {
                schemas.Add(schema);
            }
        }

        /// <summary>
        /// Loads external schemas referenced by the specified <paramref name="schema"/>.
        /// </summary>
        /// <param name="schema">XML schema that may contain references to external schemas.</param>
        /// <param name="externalSchemas">Dictionary of already loaded external schemas.</param>
        /// <param name="topSchema">Root XML schema.</param>
        /// <remarks>
        /// External schemas are those schemas referenced by import and include schema elements.
        /// </remarks>
        private void ReadExternalSchemas(XmlSchema schema, Dictionary<string, XmlSchema> externalSchemas, XmlSchema topSchema)
        {
            foreach (XmlSchemaExternal external in schema.Includes)
            {
                if (external.Schema != null || string.IsNullOrEmpty(external.SchemaLocation))
                {
                    continue;
                }

                external.SchemaLocation = ResolvePath(schema.SourceUri, external.SchemaLocation);
                if (topSchema.SourceUri == external.SchemaLocation)
                {
                    external.Schema = new XmlSchema();
                    external.Schema.TargetNamespace = schema.TargetNamespace;
                    external.SchemaLocation = null;
                    break; // TODO: why? shouldn't we continue processing remaning includes?
                }

                if (externalSchemas.ContainsKey(external.SchemaLocation))
                {
                    external.Schema = externalSchemas[external.SchemaLocation];
                }
                else if (File.Exists(external.SchemaLocation))
                {
                    external.Schema = this.ReadSchema(external.SchemaLocation);
                    externalSchemas[external.SchemaLocation] = external.Schema;
                    this.ReadExternalSchemas(external.Schema, externalSchemas, topSchema);
                }

                if (external.Schema != null)
                {
                    external.SchemaLocation = null; // TODO: why?
                }
            }
        }

        /// <summary>
        /// Reads a schema from file.
        /// </summary>
        /// <param name="filePath">Name and path of the schema file.</param>
        /// <returns>XML schema loaded from the file.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "stream is not closed by reader")]
        private XmlSchema ReadSchema(string filePath)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.XmlResolver = null;
            settings.CloseInput = false;

            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (XmlReader reader = XmlReader.Create(stream, settings))
            {
                this.schemaValidationErrors = false;
                try
                {
                    XmlSchema schema = XmlSchema.Read(reader, this.ValidationErrorHandler);
                    if (this.schemaValidationErrors)
                    {
                        // TODO: Does this ever execute?
                        DirectiveProcessorException e = new DirectiveProcessorException(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                "schema '{0}' contains validation errors", 
                                filePath));
                        e.Source = filePath;
                        throw e;
                    }

                    schema.SourceUri = filePath;
                    return schema;
                }
                catch (XmlException e)
                {
                    DirectiveProcessorException directiveException = new DirectiveProcessorException(e.Message, e);
                    directiveException.Source = filePath;
                    throw directiveException;
                }
            }
        }

        /// <summary>
        /// Handles validation errors that occur during loading and compillation of XML schemas.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="args">Validation error details.</param>
        private void ValidationErrorHandler(object sender, ValidationEventArgs args)
        {
            // Determine schema file name
            XmlSchemaObject schemaObject = args.Exception.SourceSchemaObject;
            while (schemaObject != null && string.IsNullOrEmpty(schemaObject.SourceUri))
            {
                schemaObject = schemaObject.Parent;
            }

            // Report validation error
            CompilerError error = new CompilerError();
            error.ErrorText = args.Message;
            error.Line = args.Exception.LineNumber;
            error.Column = args.Exception.LinePosition;
            error.IsWarning = args.Severity == XmlSeverityType.Warning;
            error.FileName = schemaObject.SourceUri;
            this.Errors.Add(error);

            this.schemaValidationErrors |= args.Severity == XmlSeverityType.Error;
        }

        #endregion
    }
}
