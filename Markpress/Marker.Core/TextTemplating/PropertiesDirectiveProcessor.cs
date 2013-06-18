namespace MarkdownContent.Core.TextTemplating
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using Microsoft.VisualStudio.TextTemplating;

    internal class PropertiesDirectiveProcessor : DirectiveProcessor, IDisposable
    {
        private const string ConverterAttribute = "converter";
        private const string EditorAttribute = "editor";
        private const string NameAttribute = "name";
        private const string PropertyDirectiveName = "property";
        private const string PropertyDirectiveProcessorName = "PropertyProcessor";
        private const string TypeAttribute = "type";

        private StringWriter codeWriter;
        private TemplateHost templateHost;
        private CodeDomProvider codeLanguageProvider;

        public override void FinishProcessingRun()
        {
        }

        public override string GetClassCodeForProcessingRun()
        {
            if (this.codeWriter == null)
            {
                throw new InvalidOperationException("A required method invocation 'StartProcessingRun()' was not invoked.");
            }

            return this.codeWriter.ToString();
        }

        public override string[] GetImportsForProcessingRun()
        {
            return null;
        }

        public override string GetPostInitializationCodeForProcessingRun()
        {
            return null;
        }

        public override string GetPreInitializationCodeForProcessingRun()
        {
            return null;
        }

        public override string[] GetReferencesForProcessingRun()
        {
            return new string[] { this.GetType().Module.Name };
        }

        public override void Initialize(ITextTemplatingEngineHost host)
        {
            base.Initialize(host);

            this.templateHost = (TemplateHost)host;
        }

        public override bool IsDirectiveSupported(string directiveName)
        {
            return string.Compare(directiveName, "property", StringComparison.OrdinalIgnoreCase) == 0;
        }

        public override void ProcessDirective(string directiveName, IDictionary<string, string> arguments)
        {
            if (string.Compare(directiveName, "property", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (!this.templateHost.Arguments.ContainsKey(arguments["name"]))
                {
                    throw new ArgumentNullException(arguments["name"], string.Format(CultureInfo.CurrentCulture, "Template uses property {0} which has not been received for execution.", new object[] { "name" }));
                }

                CodeMemberProperty member = new CodeMemberProperty
                {
                    Name = arguments["name"]
                };

                if (arguments.ContainsKey("type"))
                {
                    member.Type = new CodeTypeReference(arguments["type"]);
                }
                else
                {
                    member.Type = new CodeTypeReference(this.templateHost.Arguments[member.Name].Type);
                }

                if (arguments.ContainsKey("converter"))
                {
                    member.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(TypeConverterAttribute)), new CodeAttributeArgument[] { new CodeAttributeArgument(new CodeTypeOfExpression(arguments["converter"])) }));
                }

                if (arguments.ContainsKey("editor"))
                {
                    member.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(System.ComponentModel.EditorAttribute)), new CodeAttributeArgument[] { new CodeAttributeArgument(new CodeTypeOfExpression(arguments["editor"])) }));
                }

                member.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                member.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(TemplatePropertyAttribute))));
                member.GetStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(member.Type, new CodePropertyReferenceExpression(new CodeIndexerExpression(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(TemplateHost)), "CurrentHost"), "Arguments"), new CodeExpression[] { new CodePrimitiveExpression(member.Name) }), "Value"))));
                CodeGeneratorOptions options = new CodeGeneratorOptions
                {
                    BracingStyle = "C"
                };

                this.codeLanguageProvider.GenerateCodeFromMember(member, this.codeWriter, options);
            }
        }

        public override void StartProcessingRun(CodeDomProvider languageProvider, string templateContents, CompilerErrorCollection errors)
        {
            base.StartProcessingRun(languageProvider, templateContents, errors);

            this.codeWriter = new StringWriter(CultureInfo.CurrentCulture);
            this.codeLanguageProvider = languageProvider;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.codeWriter != null)
                {
                    this.codeWriter.Close();
                    this.codeWriter = null;
                }
            }
        }
    }
}
