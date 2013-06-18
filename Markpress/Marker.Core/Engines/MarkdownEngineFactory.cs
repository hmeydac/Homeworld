namespace MarkdownContent.Core.Engines
{
    using ContentFramework.Model;

    public static class MarkdownEngineFactory
    {
        public static IMarkdownEngine GetEngine(ConvertibleDocument document)
        {
            switch (document.MarkdownEngine)
            {
                case MarkdownEngine.Sundown:
                    return new SundownEngine();
                case MarkdownEngine.MarkdownSharp:
                    return new MarkdownSharpEngine();
            }

            return null;
        }
    }
}
