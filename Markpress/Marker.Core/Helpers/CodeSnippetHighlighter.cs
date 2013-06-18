namespace MarkdownContent.Core.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Wilco.SyntaxHighlighting;

    public class CodeSnippetHighlighter
    {
        private static object syncRoot = new object();
        private static object innerHtmlSyncRoot = new object();
        private HtmlParser parser;
        private string originalText;

        public CodeSnippetHighlighter(string htmlText)
        {
            this.parser = new HtmlParser(htmlText);
            this.originalText = htmlText;
        }

        public string Highlight()
        {
            try
            {
                var codeSnippetBlocks = this.parser.GetElements("code");
                if (codeSnippetBlocks == null)
                {
                    return this.originalText;
                }

                int currentCount = 1;
                int total = codeSnippetBlocks.Count();

                Parallel.ForEach(
                    codeSnippetBlocks,
                    codeBlock =>
                    {
                        var originalOuterHtml = codeBlock.OuterHtml;
                        try
                        {

                            // Notifying progress                   
                            ProgressNotificationHelper.ReportProgress(currentCount / total, "Processing Code Snippets");
                            currentCount++;
                            var syntaxProcessor = new SyntaxHighlighterProcessor();
                            this.HighlightCodeSnippet(syntaxProcessor, codeBlock);
                        }
                        catch (Exception ex)
                        {
                            var message = string.Format(
                                CultureInfo.CurrentCulture,
                                "There was an error highlighting code!{2}Code Block OuterHtml: '{0}'{2}Original OuterHtml: '{1}'{2}",
                                codeBlock.OuterHtml,
                                originalOuterHtml,
                                Environment.NewLine);
                            throw new InvalidOperationException(message, ex);
                        }

                    });
            }
            catch (Exception ex)
            {
                throw new Exception("Error while trying to highlight a block of code", ex);
            }

            return this.parser.Html;
        }

        public int[] GetLinesFromCommand(HtmlAgilityPack.HtmlNode htmlNode, string commandName)
        {
            if (htmlNode == null)
            {
                return null;
            }

            var text = htmlNode.InnerText;
            text = text.Replace("<!--", string.Empty).Replace("-->", string.Empty);
            var command = text.Split(';').Where(s => s.Trim().StartsWith(commandName)).FirstOrDefault();

            if (command == null)
            {
                return null;
            }

            try
            {
                var arguments = command.Split(':')[1];
                var lineRanges = arguments.Split(',');
                var lineSet = new System.Collections.Generic.SortedSet<int>();

                foreach (var range in lineRanges)
                {
                    this.ParseRange(lineSet, range);
                }

                return lineSet.ToArray();
            }
            catch
            {
                return null;
            }
        }

        private void HighlightCodeSnippet(SyntaxHighlighterProcessor syntaxProcessor, HtmlAgilityPack.HtmlNode codeBlock)
        {
            // The parent node of a codeblock is a PRE element. Find if there is a Comment above the PRE tag to parse.     
            HtmlAgilityPack.HtmlNode previousElement = null;
            int[] linesToMark;
            int[] linesToStrikethrough;

            lock (syncRoot)
            {
                previousElement = this.parser.FindPreviousElement(codeBlock.ParentNode);
                if (previousElement != null && previousElement.Name == "span")
                {
                    previousElement = this.parser.FindPreviousElement(previousElement, HtmlAgilityPack.HtmlNodeType.Comment);
                }
            }

            if (previousElement == null || previousElement.NodeType != HtmlAgilityPack.HtmlNodeType.Comment)
            {
                linesToMark = new int[] { };
                linesToStrikethrough = new int[] { };
            }
            else
            {
                linesToMark = this.GetLinesFromCommand(previousElement, "mark");
                linesToStrikethrough = this.GetLinesFromCommand(previousElement, "strike");
            }

            if (!codeBlock.Attributes.Contains("class"))
            {
                return;
            }

            var language = codeBlock.Attributes["class"].Value;
            var codeSnippetText = codeBlock.InnerHtml;

            var highlightedSnippet = syntaxProcessor.Highlight(codeSnippetText, language, linesToMark, linesToStrikethrough);

            // intentional: The Dictionary behind the InnerHtml manipulation in the HtmlDocument class is not thread safe
            lock (innerHtmlSyncRoot)
            {
                codeBlock.InnerHtml = highlightedSnippet;
            }
        }

        private void ParseRange(SortedSet<int> lineSet, string range)
        {
            if (range.Contains('-'))
            {
                var fromRange = int.Parse(range.Split('-')[0]);
                var toRange = int.Parse(range.Split('-')[1]);

                for (var i = fromRange; i <= toRange; i++)
                {
                    lineSet.Add(i);
                }
            }
            else
            {
                lineSet.Add(int.Parse(range));
            }
        }
    }
}