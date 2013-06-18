namespace MarkdownContent.Core
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using ContentFramework.Model;
    using HtmlAgilityPack;
    using MarkdownContent.Core.Engines;
    using MarkdownContent.Core.Helpers;

    public class MarkdownToHtmlConverter
    {
        private const string CodeHeaderTemplate = "<span class=\"codelanguage\">{0}</span>";

        private readonly string[] acceptedSourceContent = new string[] { @"*images\*", @"*img\*", @"*styles\*" };
        private readonly string[] rejectedSourceContent = null;

        private readonly string[] acceptedTemplateContent = null;
        private readonly string[] rejectedTemplateContent = new string[] { "*.tt" };

        // In case we are interacting with an UI, we can notify progress using the BackgroundWorker.
        public void NotifyProgress(BackgroundWorker worker)
        {
            ProgressNotificationHelper.Initialize(worker);
        }

        public void Convert(ConvertibleDocument document, ConversionMetadata conversionMetadata)
        {
            if (string.IsNullOrEmpty(conversionMetadata.OutputPath))
            {
                conversionMetadata.OutputPath = document.BasePath;
            }

            if (Path.GetFullPath(Path.GetDirectoryName(document.AbsolutePath)) != Path.GetFullPath(conversionMetadata.OutputPath))
            {
                this.CopySourceContent(document.AbsolutePath, conversionMetadata.OutputPath);
            }

            if (!string.IsNullOrEmpty(conversionMetadata.MarkdownTemplatePath)
                && Path.GetFullPath(conversionMetadata.MarkdownTemplatePath) != Path.GetFullPath(conversionMetadata.OutputPath))
            {
                this.CopyTemplateContent(conversionMetadata.MarkdownTemplatePath, conversionMetadata.OutputPath);
            }

            this.Render(document, conversionMetadata);
            ProgressNotificationHelper.Stop();
        }

        private string Render(ConvertibleDocument document, ConversionMetadata conversionMetadata)
        {
            var engine = MarkdownEngineFactory.GetEngine(document);
            var fileName = Path.GetFileName(Path.ChangeExtension(document.AbsolutePath, "html"));

            string markdownContent;
            using (var stream = File.Open(document.AbsolutePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
                markdownContent = reader.ReadToEnd();
            }

            var outputContent = engine.Transform(markdownContent);
            outputContent = this.ProcessHeaders(outputContent);
            outputContent = this.RemoveCodeSnippetExtraBreakline(outputContent);
            outputContent = this.PrcessCodeSnippetHeaders(outputContent);

            if (conversionMetadata.CodeSnippetsHighlighting)
            {
                outputContent = new CodeSnippetHighlighter(outputContent).Highlight();
            }

            document.Topics.Clear();
            document.Topics.Add(new DocumentTopic { Content = outputContent });

            if (!string.IsNullOrEmpty(conversionMetadata.MarkdownTemplatePath))
            {
                var props = new Dictionary<string, object>();
                props.Add("Document", document);
                props.Add("Content", conversionMetadata.Content);
                props.Add("Package", conversionMetadata.Package);

                outputContent = TextTemplatingHelper.Process(conversionMetadata.MarkdownTemplatePath, props);
            }

            string outputPath = Path.Combine(conversionMetadata.OutputPath, fileName);

            File.WriteAllText(outputPath, outputContent, System.Text.Encoding.UTF8);

            ProgressNotificationHelper.Clear();

            return outputPath;
        }

        private string PrcessCodeSnippetHeaders(string content)
        {
            var parser = new HtmlParser(content);
            var codeSnippetBlocks = parser.GetElements("code");

            if (codeSnippetBlocks == null)
            {
                return content;
            }

            string language = string.Empty;
            int currentCount = 1;
            int total = codeSnippetBlocks.Count();

            foreach (var codeBlock in codeSnippetBlocks)
            {
                // Notifying progress                   
                ProgressNotificationHelper.ReportProgress(currentCount / total, "Processing Code Snippets Headers");
                currentCount++;
                if (!codeBlock.Attributes.Contains("class"))
                {
                    continue;
                }

                language = codeBlock.Attributes["class"].Value;
                if (!string.IsNullOrWhiteSpace(language))
                {
                    codeBlock.ParentNode.ParentNode.InsertBefore(
                        HtmlNode.CreateNode(string.Format(CodeHeaderTemplate, language)), 
                        codeBlock.ParentNode);
                }
            }

            return parser.Html;
        }

        private string RemoveCodeSnippetExtraBreakline(string content)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(content);

            var highlightedLines = doc.DocumentNode.SelectNodes("//p/code");
            if (highlightedLines != null)
            {
                int currentCount = 1;
                int total = highlightedLines.Count;
                ProgressNotificationHelper.Clear();
                foreach (var line in highlightedLines)
                {
                    // Notifying progress
                    ProgressNotificationHelper.ReportProgress(currentCount / total, "Processing Breaklines");
                    currentCount += 1;

                    if (line.InnerHtml.Substring(0, 1) == "\n")
                    {
                        line.InnerHtml = line.InnerHtml.Substring(1);
                    }
                }

                using (var writer = new StringWriter())
                {
                    doc.Save(writer);
                    return writer.ToString();
                }
            }
            else
            {
                return content;
            }
        }

        /// <summary>
        /// Takes HTML and parses out all heading and sets IDs for each heading.
        /// </summary>
        private string ProcessHeaders(string content)
        {
            var doc = new HtmlDocument();
            doc.OptionUseIdAttribute = true;
            doc.LoadHtml(content);

            var allNodes = doc.DocumentNode.DescendantNodes();
            var allHeadingNodes = allNodes.Where(node =>
                node.Name.Length == 2
                && node.Name.StartsWith("h", System.StringComparison.InvariantCultureIgnoreCase)
                && !node.Name.Equals("hr", StringComparison.InvariantCultureIgnoreCase)).ToList();

            int currentCount = 1;
            int total = allHeadingNodes.Count;
            ProgressNotificationHelper.Clear();
            foreach (var heading in allHeadingNodes)
            {
                // Notifying progress
                ProgressNotificationHelper.ReportProgress(currentCount / total, "Processing Headers");
                currentCount += 1;
                var id = Regex.Replace(HttpUtility.HtmlDecode(heading.InnerHtml).Replace(" ", "_"), @"[^\w\-0-9]", string.Empty);
                id = HttpUtility.HtmlAttributeEncode(id);
                heading.SetAttributeValue("id", id);
            }

            var docteredHTML = new StringWriter();
            doc.Save(docteredHTML);

            return docteredHTML.ToString();
        }

        private void CopyTemplateContent(string templatePath, string outputFolder)
        {
            var templateFolder = Path.GetDirectoryName(templatePath);
            DirectoryHelper.CopyDirectory(templateFolder, outputFolder, this.rejectedTemplateContent, this.acceptedTemplateContent);
        }

        private void CopySourceContent(string sourcePath, string outputFolder)
        {
            var sourceFolder = Path.GetDirectoryName(sourcePath);
            DirectoryHelper.CopyDirectory(sourceFolder, outputFolder, this.rejectedSourceContent, this.acceptedSourceContent);
        }
    }
}
