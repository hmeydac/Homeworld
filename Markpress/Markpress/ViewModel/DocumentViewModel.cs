namespace Markpress.ViewModel
{
    using Markpress.Processor;

    public class DocumentViewModel
    {
        public DocumentViewModel()
        {
            this.Document = DocumentHub.NewDocument();
        }

        public Document Document { get; set; }
    }
}