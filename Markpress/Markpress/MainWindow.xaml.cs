namespace Markpress
{
    using System;
    using System.Windows.Forms.Integration;

    using Markpress.Controller;
    using Markpress.ViewModel;

    using ScintillaNet;
    using Markpress.Processor;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly MarkpressViewModel markpressContext;

        private readonly PreviewController previewController;

        private Scintilla codeEditorControl;

        public MainWindow()
        {
            this.InitializeComponent();
            this.markpressContext = new MarkpressViewModel();
            this.DataContext = this.markpressContext;
            this.previewController = new PreviewController(this.Previewer);
        }

        private DocumentViewModel DocumentContext
        {
            get
            {
                return this.markpressContext.DocumentContext;
            }
        }

        private void WindowLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Create the interop host control.
            var host = new WindowsFormsHost();
            this.codeEditorControl = new Scintilla { MatchBraces = true };
            this.codeEditorControl.DocumentChange += this.CodeEditorControlTextChanged;

            // Assign the MaskedTextBox control as the host control's child.
            host.Child = this.codeEditorControl;

            // Add the interop host control to the Grid 
            // control's collection of child controls. 
            this.CodeEditorGrid.Children.Add(host);
        }

        private void CodeEditorControlTextChanged(object sender, EventArgs e)
        {
            this.DocumentContext.Document.Text = this.codeEditorControl.Text;
            if (!this.DocumentContext.Document.HasChanges)
            {
                this.DocumentContext.Document.HasChanges = true;
            }

            this.previewController.UpdatePreview(this.DocumentContext.Document);
        }
    }
}
