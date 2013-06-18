namespace Markpress.Controller
{
    using System;
    using System.IO;
    using System.Windows.Controls;

    using Markpress.Processor;

    public class PreviewController
    {
        private const string HtmlHeader = @"<html><meta http-equiv='Content-Type' content='text/html;charset=UTF-8'><body>";
        private const string HtmlFooter = @"</body></html>";

        private readonly WebBrowser previewer;

        private FileStream fileStream;

        public PreviewController(WebBrowser previewControl)
        {
            this.previewer = previewControl;
        }

        public void UpdatePreview(Document document)
        {
            var parsedHtml = MarkdownParser.ToHtml(document.Text);
            if (string.IsNullOrEmpty(parsedHtml))
            {
                return;
            }

            var htmlBody = string.Format("{0}{1}{2}", HtmlHeader, parsedHtml, HtmlFooter);

            this.fileStream = new FileStream(document.PreviewFileName, FileMode.Create, FileAccess.ReadWrite);
            var writer = new StreamWriter(this.fileStream);
            writer.Write(htmlBody);
            writer.Close();
            var reader = new Uri(document.PreviewFileName);
            this.previewer.Navigate(reader);
            this.fileStream.Close();
        }
    }
}
