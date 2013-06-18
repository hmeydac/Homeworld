namespace MarkdownContent.Core.TextTemplating
{
    using System;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class TemplatePropertyAttribute : Attribute
    {
    }
}
