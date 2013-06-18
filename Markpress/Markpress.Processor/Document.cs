using System.IO;
namespace Markpress.Processor
{
    public class Document
    {
        public Document()
        {
            this.HasChanges = false;
            this.IsTemporal = true;
        }

        public string FileName { get; set; }

        public string FolderPath { get; set; }

        public string FullFilename
        {
            get
            {
                return Path.Combine(this.FolderPath, this.FileName);
            }
        }

        public string PreviewFileName { get; set; }

        public bool HasChanges { get; set; }

        public bool IsTemporal { get; set; }

        public string Text { get; set; }
    }
}
