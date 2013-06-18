namespace Markpress.Commands
{
    using System;
    using System.Windows.Forms;
    using System.Windows.Input;

    using Markpress.Processor;
    using Markpress.Properties;
    using Markpress.ViewModel;

    public class NewCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            var context = parameter as DocumentViewModel;
            return context != null;
        }

        public void Execute(object parameter)
        {
            var context = (DocumentViewModel)parameter;
            if (!context.Document.HasChanges)
            {
                return;
            }

            var confirm = MessageBox.Show(
                Resources.PendingChanges,
                Resources.PendingChangesCaption,
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Exclamation);

            switch (confirm)
            {
                case DialogResult.Yes:
                    {
                        var saved = DocumentHub.SaveDocument(context.Document);
                        if (saved)
                        {
                            context.Document = DocumentHub.NewDocument();
                        }

                        break;
                    }

                case DialogResult.No:
                    {
                        context.Document = DocumentHub.NewDocument();
                        break;
                    }

                case DialogResult.Cancel:
                    {
                        return;
                    }
            }
        }
    }
}
