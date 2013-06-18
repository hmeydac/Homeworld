namespace MarkdownContent.Core.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class CodeSnippet
    {
        private string snippetTemplate;

        private string contentTemplate;

        public CodeSnippet(string snippetTemplate, string contentTemplate)
        {
            this.snippetTemplate = snippetTemplate;
            this.contentTemplate = contentTemplate;
        }

        public string Language { get; set; }

        public string Code { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string Filename
        {
            get
            {
                string res = null;

                res = this.Title.Replace('-', ' ');
                res = res.Replace('–', ' ');

                for (int index = 1; index <= 5; index++)
                {
                    res = res.Replace("  ", " ");
                }

                res = res.Replace(" ", string.Empty);
                res = res.Replace(".", string.Empty);

                return res;
            }
        }

        public string GetSnippetContent()
        {
            string content = null;

            content = this.snippetTemplate;
            content = content.Replace("{{title}}", this.Title);
            content = content.Replace("{{language}}", this.Language);
            content = content.Replace("{{code}}", this.Code);
            content = content.Replace("{{author}}", this.Author);
            content = content.Replace("{{shortcut}}", this.Filename);

            return content;
        }

        public string GetVSContent()
        {
            string content = null;

            content = this.contentTemplate;
            content = content.Replace("{{title}}", this.Title);
            content = content.Replace("{{filename}}", this.Filename);
            content = content.Replace("{{language}}", this.Language);

            return content;
        }
    }
}
