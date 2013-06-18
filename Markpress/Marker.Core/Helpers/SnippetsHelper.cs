namespace MarkdownContent.Core.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using HtmlAgilityPack;
    using MarkdownContent.Core.Model;

    public class SnippetsHelper
    {
        public bool Cancel
        {
            get;
            set;
        }

        public string Author
        {
            get;
            set;
        }

        public string[] ExternalSnippets
        {
            get;
            set;
        }

        public string HTMLCode
        {
            get;
            set;
        }

        public string VSIPath { get; set; }

        public string VSContentTemplate { get; set; }

        public string SnippetTemplate { get; set; }

        public List<CodeSnippet> GetCodeSnippets()
        {
            return this.GetCodeSnippets(this.HTMLCode, this.ExternalSnippets);
        }

        public List<CodeSnippet> GetCodeSnippets(string markdownContent)
        {
            return this.GetCodeSnippets(markdownContent, null);
        }

        public List<CodeSnippet> GetCodeSnippets(string markdownContent, string[] externalSnippets)
        {
            var parser = new HtmlParser(markdownContent);
            List<CodeSnippet> snippets = default(List<CodeSnippet>);
            snippets = this.ReadSnippets(parser);
            snippets.AddRange(this.GetExternalSnippets());
            return snippets;
        }

        public List<CodeSnippet> GetExternalSnippets()
        {
            List<CodeSnippet> externalSnippesList = new List<CodeSnippet>();
            if (this.ExternalSnippets != null)
            {
                StepNotificationHelper.Initialize(this.ExternalSnippets.Count() + 1);
                StepNotificationHelper.Step("Reading External Snippets");
                foreach (string snippetPath in this.ExternalSnippets)
                {
                    StepNotificationHelper.Step();
                    CodeSnippet s = this.ReadSnippetFromFile(snippetPath);
                    externalSnippesList.Add(s);
                }
            }

            return externalSnippesList;
        }

        private CodeSnippet ReadSnippetFromFile(string snippetPath)
        {
            CodeSnippet snippet = new CodeSnippet(this.SnippetTemplate, this.VSContentTemplate);
            string snippetContent = File.ReadAllText(snippetPath);

            var matchTitle = Regex.Match(snippetContent, "<title\\b[^>]*>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var matchLang = Regex.Match(snippetContent, "<code language=\"(.*)\">", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var codeStart = snippetContent.IndexOf(matchLang.Value) + matchLang.Value.Length;
            var codeEnd = snippetContent.ToLower().LastIndexOf("</code>");

            if (snippetContent.Contains("<![CDATA["))
            {
                codeStart = snippetContent.IndexOf("<![CDATA[", codeStart) + 9;
                codeEnd = snippetContent.LastIndexOf("]]>");
            }

            snippet.Title = matchTitle.Groups[1].Value;
            snippet.Language = matchLang.Groups[1].Value;
            snippet.Code = snippetContent.Substring(codeStart, codeEnd - codeStart);

            return snippet;
        }

        private List<CodeSnippet> ReadSnippets(HtmlParser parser)
        {
            List<CodeSnippet> codeSnippetsList = new List<CodeSnippet>();
            var codeNodes = parser.GetElements("pre/code");

            if (codeNodes == null)
            {
                return codeSnippetsList;
            }

            StepNotificationHelper.Initialize(codeNodes.Count() + 1);
            StepNotificationHelper.Step("Scanning snippets...");

            foreach (HtmlNode codeElement in codeNodes)
            {
                if (this.Cancel)
                {
                    throw new OperationCanceledException("Operation Cancelled");
                }

                StepNotificationHelper.Step();
                var previousElement = parser.FindPreviousElement(codeElement.ParentNode);
                if (previousElement != null && previousElement.Name == "span")
                {
                    previousElement = parser.FindPreviousElement(previousElement);
                }

                if (previousElement == null || (previousElement.Name != "p" && previousElement.NodeType != HtmlNodeType.Comment))
                {
                    continue;
                }

                HtmlNode commentNode = null;
                if (previousElement.NodeType == HtmlNodeType.Comment)
                {
                    commentNode = previousElement;
                    previousElement = parser.FindPreviousElement(previousElement);
                    if (previousElement.Name != "p" || !previousElement.InnerText.StartsWith("(Code Snippet"))
                    {
                        continue;
                    }
                }

                // Check that title format is "(Code Snippet - title goes here)"
                Regex codeSnippetRegex = new Regex(@"^\(Code Snippet \p{Pd} .+\)$");
                if (!codeSnippetRegex.IsMatch(previousElement.InnerText))
                {
                    continue;
                }

                // Check that the language name is not null nor an empty string
                string languageName;
                if (codeElement.Attributes["Class"] != null)
                {
                    languageName = HttpUtility.HtmlDecode(codeElement.Attributes["Class"].Value).Trim();
                    languageName = this.StandardizeCodeLanguage(languageName);

                    if (languageName == string.Empty)
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }

                // Search for the code snippet title 
                string codeSnippetTitle = HttpUtility.HtmlDecode(previousElement.InnerText);
                codeSnippetTitle = codeSnippetTitle.Substring(16, codeSnippetTitle.Length - 17);

                CodeSnippet codeSnippet = new CodeSnippet(this.SnippetTemplate, this.VSContentTemplate)
                {
                    Code = HttpUtility.HtmlDecode(codeElement.InnerText.Trim()),
                    Language = languageName,
                    Title = codeSnippetTitle,
                    Author = this.Author
                };

                var commentLine = string.Empty;
                if (commentNode != null)
                {
                    commentLine = commentNode.InnerText.Replace("<!--", string.Empty).Replace("-->", string.Empty);
                }

                try
                {
                    this.WrapUpSnippetCode(codeSnippet, commentLine);
                    codeSnippet.Code = codeSnippet.Code.Trim();
                    codeSnippetsList.Add(codeSnippet);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return codeSnippetsList;
        }

        private void WrapUpSnippetCode(CodeSnippet codeSnippet, string commentLine)
        {
            // If the commentLine is empty, then all the snippet should be inserted (default: all bold)
            if (commentLine != string.Empty)
            {
                var linesToMark = new SortedSet<int>();
                var strikedLines = new SortedSet<int>();

                // These are the lines that are bolded
                linesToMark = this.GetLinesFromCommand(commentLine, "mark");

                try
                {
                    // These are the lines that are strikethrough
                    strikedLines = this.GetLinesFromCommand(commentLine, "strike");
                }
                catch (Exception)
                {
                    strikedLines = null;
                }

                if (strikedLines != null)
                {
                    // remove the strikedthrough lines from the list
                    linesToMark.ExceptWith(strikedLines.ToList());
                }

                codeSnippet.Code = this.RemoveExtraLines(linesToMark, codeSnippet.Code);
            }
        }

        private SortedSet<int> GetLinesFromCommand(string commentLine, string commandName)
        {
            var command = commentLine.Split(';').Where(s => s.Trim().StartsWith(commandName)).FirstOrDefault();
            var arguments = command.Split(':')[1];
            var values = arguments.Replace("\r\n", string.Empty).Trim().Split(',');
            var linesToMark = new SortedSet<int>();
            this.ScanValues(values, linesToMark);
            return linesToMark;
        }

        private string RemoveExtraLines(SortedSet<int> linesToMark, string code)
        {
            var codeLines = code.Replace("\r", string.Empty).Split('\n');

            if (linesToMark == null || codeLines.Count() == linesToMark.Count)
            {
                return code;
            }

            var builder = new StringBuilder();

            foreach (var lineNum in linesToMark)
            {
                builder.AppendLine(codeLines[lineNum - 1]);
            }

            return builder.ToString();
        }

        private void ScanValues(string[] values, SortedSet<int> linesToMark)
        {
            foreach (var value in values)
            {
                if (value.Contains("-"))
                {
                    this.AddRangeValues(linesToMark, value);
                }
                else
                {
                    this.AddSingleValue(linesToMark, value);
                }
            }
        }

        private void AddRangeValues(SortedSet<int> linesToMark, string value)
        {
            var extremes = value.Split('-');
            var min = int.Parse(extremes[0]);
            var max = int.Parse(extremes[1]);

            for (var i = min; i <= max; i++)
            {
                if (!linesToMark.Contains(i))
                {
                    linesToMark.Add(i);
                }
            }
        }

        private void AddSingleValue(SortedSet<int> linesToMark, string value)
        {
            var lineNumber = int.Parse(value);
            if (!linesToMark.Contains(lineNumber))
            {
                linesToMark.Add(lineNumber);
            }
        }

        private string StandardizeCodeLanguage(string codeName)
        {
            if (codeName.Equals("VISUAL BASIC", StringComparison.OrdinalIgnoreCase) || codeName.Equals("VB", StringComparison.OrdinalIgnoreCase))
            {
                codeName = "vb";
            }
            else if (codeName.Equals("C#", StringComparison.OrdinalIgnoreCase))
            {
                codeName = "csharp";
            }
            else if (codeName.Equals("XAML", StringComparison.OrdinalIgnoreCase))
            {
                codeName = "xml";
            }
            else if (codeName.Equals("ASPX", StringComparison.OrdinalIgnoreCase) || codeName.Equals("ASP.NET", StringComparison.OrdinalIgnoreCase) || codeName.Equals("ASPNET", StringComparison.OrdinalIgnoreCase) || codeName.Equals("HTML", StringComparison.OrdinalIgnoreCase) || codeName.Equals("CSHTML", StringComparison.OrdinalIgnoreCase))
            {
                codeName = "html";
            }

            return codeName;
        }
    }
}
