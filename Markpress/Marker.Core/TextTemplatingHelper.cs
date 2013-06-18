namespace MarkdownContent.Core
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using MarkdownContent.Core.TextTemplating;

    public class TextTemplatingHelper
    {
        private const string TextTemplatingAssemblyNameVS2008 = "Microsoft.VisualStudio.TextTemplating.dll";
        private const string TextTemplatingEngineAssemblyNameVS2010 = "Microsoft.VisualStudio.TextTemplating.10.0.dll";
        private const string TextTemplatingInterfacesAssemblyNameVS2010 = "Microsoft.VisualStudio.TextTemplating.Interfaces.10.0.dll";

        public static bool IsTextTemplatingInstalled 
        {
            get
            {
                if (CheckTextTemplatingAssembly(TextTemplatingEngineAssemblyNameVS2010) && CheckTextTemplatingAssembly(TextTemplatingInterfacesAssemblyNameVS2010))
                {
                    return true;
                }                

                return CheckTextTemplatingAssembly(TextTemplatingAssemblyNameVS2008);
            }
        }

        public static string Process(string templateFile, Dictionary<string, object> properties)
        {
            var templateProcessor = new SimpleTemplateProcessor();
            var processedContent = templateProcessor.Execute(properties, templateFile);

            if (templateProcessor.Errors.Count > 0)
            {
                throw new Exception(string.Format(CultureInfo.CurrentCulture, "Template processing has failed: {0}", templateProcessor.ErrorsString));
            }

            return processedContent;
        }

        private static bool CheckTextTemplatingAssembly(string assemblyName)
        {
            var assemblyExists = File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), assemblyName));
            
            if (assemblyExists)
            {
                return true;
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assembly = (from a in assemblies
                            where a.ManifestModule.Name == assemblyName
                            select a).SingleOrDefault();

            return assembly != null;
        }
    }
}