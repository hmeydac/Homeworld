namespace MarkdownContent.Core.TextTemplating
{
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using Microsoft.VisualStudio.TextTemplating;

    public class SimpleTemplateProcessor
    {
		private static object syncRoot = new object();
        private TemplateHost host;

        public SimpleTemplateProcessor()
        {
        }

        public virtual CompilerErrorCollection Errors
        {
            get
            {
                return this.host.Errors;
            }
        }

        public virtual string ErrorsString
        {
            get
            {
                StringBuilder buffer = new StringBuilder();
                foreach (CompilerError error in this.host.Errors)
                {
                    buffer.AppendLine(error.ToString());
                }

                return buffer.ToString();
            }
        }

        public virtual string Execute(Dictionary<string, object> properties, string templateFile)
        {
			lock (syncRoot)
            {
				this.host = new TemplateHost(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), MapProperties(properties));
				this.host.TemplateFile = templateFile;

				ITextTemplatingEngine engine = new Engine();
				string template = File.ReadAllText(this.host.TemplateFile);
				string result = engine.ProcessTemplate(template, this.host);

				return result;
			}
        }

        private static Dictionary<string, PropertyData> MapProperties(Dictionary<string, object> properties)
        {
            Dictionary<string, PropertyData> propertiesData = new Dictionary<string, PropertyData>();

            if (properties != null)
            {
                foreach (var item in properties)
                {
                    propertiesData.Add(item.Key, new PropertyData(item.Value, item.Value.GetType()));
                }
            }

            return propertiesData;
        }
    }
}
