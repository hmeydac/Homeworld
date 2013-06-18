namespace Markpress.Commands
{
    using System;
    using System.Windows;
    using System.Windows.Input;

    public class OpenCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}
