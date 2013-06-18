namespace MarkdownContent.Core.Engines
{
    public class SundownEngine : IMarkdownEngine
    {
        public string Transform(string input)
        {
            var extensions = Sundown.MarkdownExtensions.AutoLink |
                Sundown.MarkdownExtensions.FencedCode |
                Sundown.MarkdownExtensions.LaxHtmlBlocks |
                Sundown.MarkdownExtensions.NoIntraEmphasis |
                Sundown.MarkdownExtensions.StrikeThrough |
                Sundown.MarkdownExtensions.Tables;
            var outputContent = Sundown.MoonShine.Markdownify(
                input,
                extensions, 
                false);

            return outputContent;
        }
    }
}
