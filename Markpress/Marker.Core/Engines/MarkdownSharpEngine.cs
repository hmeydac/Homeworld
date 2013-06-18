namespace MarkdownContent.Core.Engines
{
    public class MarkdownSharpEngine : IMarkdownEngine
    {
        public string Transform(string input)
        {
            var md = new MarkdownSharp.Markdown();
            var outputContent = md.Transform(input);

            return outputContent;
        }
    }
}
