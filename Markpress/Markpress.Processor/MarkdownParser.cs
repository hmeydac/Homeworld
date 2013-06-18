namespace Markpress.Processor
{
    using Sundown;

    public class MarkdownParser
    {
        private const MarkdownExtensions Extensions = MarkdownExtensions.AutoLink |
                                                  MarkdownExtensions.FencedCode |
                                                  MarkdownExtensions.LaxHtmlBlocks |
                                                  MarkdownExtensions.NoIntraEmphasis |
                                                  MarkdownExtensions.StrikeThrough |
                                                  MarkdownExtensions.Tables;

        public static string ToHtml(string markdownInput)
        {
            return MoonShine.Markdownify(markdownInput, Extensions, false);
        }
    }
}
