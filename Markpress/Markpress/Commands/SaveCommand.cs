namespace Markpress.Commands
{
    using System;
    using System.Windows.Input;

    using Markpress.Processor;
    using Markpress.ViewModel;

    public class SaveCommand : ICommand
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

            DocumentHub.SaveDocument(context.Document);
        }
    }
}
