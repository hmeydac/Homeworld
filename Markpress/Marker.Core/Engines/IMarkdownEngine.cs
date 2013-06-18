namespace MarkdownContent.Core.Engines
{
    public interface IMarkdownEngine
    {
        string Transform(string input);
    }
}
