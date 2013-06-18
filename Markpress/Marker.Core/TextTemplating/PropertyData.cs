namespace MarkdownContent.Core.TextTemplating
{
    using System;

    public class PropertyData
    {
        private Type type;
        private object value;

        public PropertyData(object value, Type type)
        {
            this.value = value;
            this.type = type;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Design Decision")]
        public Type Type
        {
            get
            {
                return this.type;
            }
        }

        public object Value
        {
            get
            {
                return this.value;
            }
        }
    }
}
