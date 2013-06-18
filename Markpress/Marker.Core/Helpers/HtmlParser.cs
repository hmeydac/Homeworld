namespace MarkdownContent.Core.Helpers
{
    using System.Collections.Generic;
    using HtmlAgilityPack;

    public class HtmlParser
    {
        private HtmlDocument htmlDocument;

        public HtmlParser(string htmlText)
        {
            this.htmlDocument = this.GetDocument(htmlText);
        }

        public string Html
        {
            get
            {
                return this.htmlDocument.DocumentNode.OuterHtml;
            }
        }

        public HtmlNode FindPreviousElement(HtmlNode node, HtmlNodeType nodeType)
        {           
            if (node.PreviousSibling != null && node.PreviousSibling.NodeType == nodeType)
            {
                return node.PreviousSibling;
            }

            if (node.ParentNode != null && node.ParentNode.NodeType == nodeType)
            {
                return node.ParentNode;
            }

            // Ignoring Text elements
            if (node.PreviousSibling != null && node.PreviousSibling.NodeType == HtmlNodeType.Text)
            {
                return this.FindPreviousElement(node.PreviousSibling, nodeType);
            }

            // Ignoring Text elements
            if (node.ParentNode != null && node.ParentNode.NodeType == HtmlNodeType.Text)
            {
                return this.FindPreviousElement(node.ParentNode, nodeType);
            }

            if (node.ParentNode != null && node.ParentNode.NodeType == nodeType)
            {
                return node.ParentNode;
            }         

            return null;
        }

        public HtmlNode FindPreviousElement(HtmlNode node)
        {
            if (node.PreviousSibling != null && node.PreviousSibling.NodeType == HtmlNodeType.Text)
            {
                return this.FindPreviousElement(node.PreviousSibling);
            }

            return node.PreviousSibling;
        }

        public IEnumerable<HtmlNode> GetElements(string elementName)
        {
            return this.GetElements(this.htmlDocument, elementName);
        }

        private IEnumerable<HtmlNode> GetElements(HtmlDocument htmlSnippet, string elementName)
        {
            var elements = new List<string>();
            elementName = "//" + elementName;
            return htmlSnippet.DocumentNode.SelectNodes(elementName);
        }

        private HtmlDocument GetDocument(string htmlText)
        {
            var htmlSnippet = new HtmlDocument();
            htmlSnippet.LoadHtml(htmlText);
            return htmlSnippet;
        }
    }
}
