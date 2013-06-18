namespace Marker.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;
    using ContentFramework.Model;   
    
    public static class DocumentMetadataHelper 
    {
        private static List<string> validFiles = new List<string> { "Lab.xml", "Demo.xml", "Sample.xml", "Whitepaper.xml", "Tutorial.xml" };

        public static DocumentMetadata GetDocumentMetadata(string documentPath)
        {
            var path = Path.GetDirectoryName(documentPath);
            DocumentMetadata retObj = null;

            var file = Directory.EnumerateFiles(path, "*.xml")
                .FirstOrDefault(f => validFiles.Contains(Path.GetFileName(f)));

            if (file != null)
            {
                var typeName = Path.GetFileNameWithoutExtension(file);

                try
                {
                    var resultType = typeof(DocumentMetadata).Assembly.GetTypes()
                        .SingleOrDefault(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
                    if (resultType != null)
                    {
                        retObj = DocumentMetadataHelper.GetDocumentMetadataContent(resultType, file);
                    }
                }
                catch
                {
                    retObj = null;
                }
            }

            return retObj;
        }

        private static Type GetDocumentMetadataContent<Type>(string filePath) where Type : DocumentMetadata
        {
            return (Type)GetDocumentMetadataContent(typeof(Type), filePath);
        }

        private static DocumentMetadata GetDocumentMetadataContent(Type type, string filePath)
        {
            using (var strm = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer serializer = new XmlSerializer(type);
                return (DocumentMetadata)serializer.Deserialize(strm);
            }
        }
    }
}
