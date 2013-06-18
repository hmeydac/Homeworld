namespace MarkdownContent.Core.TextTemplating
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Microsoft.VisualStudio.TextTemplating;

    public class TemplateHost : MarshalByRefObject, ITextTemplatingEngineHost
    {
        private const string PropertyDirectiveProcessorName = "PropertyProcessor";
        private IDictionary<string, PropertyData> arguments;
        private string binPath;
        private CompilerErrorCollection errorCollection;

        public TemplateHost(string binPath, IDictionary<string, PropertyData> arguments)
        {
            this.binPath = new DirectoryInfo(binPath).FullName;
            this.arguments = arguments;
            CurrentHost = this;
        }

        public static TemplateHost CurrentHost
        {
            get;
            set;
        }

        public IDictionary<string, PropertyData> Arguments
        {
            get
            {
                return this.arguments;
            }
        }

        public CompilerErrorCollection Errors
        {
            get
            {
                return this.errorCollection;
            }
        }

        public IList<string> StandardAssemblyReferences
        {
            get
            {
                return null;
            }
        }

        public IList<string> StandardImports
        {
            get
            {
                return null;
            }
        }

        public string TemplateFile
        {
            get;
            set;
        }

        public object GetHostOption(string optionName)
        {
            return null;
        }

        public bool LoadIncludeText(string requestFileName, out string content, out string location)
        {
            location = this.ResolveFileName(requestFileName);

            content = File.ReadAllText(location);
            return true;
        }

        public void LogErrors(CompilerErrorCollection errors)
        {
            this.errorCollection = errors;
        }

        public AppDomain ProvideTemplatingAppDomain(string content)
        {
            return AppDomain.CurrentDomain;
        }

        public string ResolveAssemblyReference(string assemblyReference)
        {
            string path = Path.Combine(this.binPath, assemblyReference);
            if (File.Exists(path))
            {
                return path;
            }

            if (!File.Exists(assemblyReference))
            {
                path = Path.Combine(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "PublicAssemblies"), assemblyReference);
                if (File.Exists(path))
                {
                    return path;
                }

                path = Path.Combine(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "PrivateAssemblies"), assemblyReference);
                if (File.Exists(path))
                {
                    return path;
                }

                if (string.IsNullOrEmpty(AppDomain.CurrentDomain.SetupInformation.PrivateBinPath))
                {
                    return assemblyReference;
                }

                foreach (string str2 in AppDomain.CurrentDomain.SetupInformation.PrivateBinPath.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    path = Path.Combine(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, str2), assemblyReference);
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
            }

            return assemblyReference;
        }

        public Type ResolveDirectiveProcessor(string processorName)
        {
            if (string.Compare(processorName, "PropertyProcessor", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return typeof(PropertiesDirectiveProcessor);
            }

            return null;
        }

        public string ResolveFileName(string fileName)
        {
            if (!Path.IsPathRooted(fileName))
            {
                string filePath = Path.Combine(this.binPath, fileName);
                if (File.Exists(filePath))
                {
                    return filePath;
                }
                else
                {
                    return Path.Combine(Path.GetDirectoryName(Path.GetFullPath(this.TemplateFile)), fileName);
                }
            }

            return fileName;
        }

        public string ResolveParameterValue(string directiveId, string processorName, string parameterName)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public string ResolvePath(string path)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public void SetFileExtension(string extension)
        {
        }

        public void SetOutputEncoding(Encoding encoding, bool fromOutputDirective)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }
    }
}
