namespace Markpress.ViewModel
{
    using System.Windows;

    using Markpress.Commands;

    public class RibbonViewModel : DependencyObject
    {
        public static readonly DependencyProperty CloseCommandProperty = DependencyProperty.Register("CloseCommand", typeof(CloseCommand), typeof(RibbonViewModel), null);
        public static readonly DependencyProperty OpenCommandProperty = DependencyProperty.Register("OpenCommand", typeof(OpenCommand), typeof(RibbonViewModel), null);
        public static readonly DependencyProperty SaveCommandProperty = DependencyProperty.Register("SaveCommand", typeof(SaveCommand), typeof(RibbonViewModel), null);
        public static readonly DependencyProperty NewCommandProperty = DependencyProperty.Register("NewCommand", typeof(NewCommand), typeof(RibbonViewModel), null);
        
        public RibbonViewModel()
        {
            this.RegisterCommands();
        }

        public OpenCommand OpenCommand
        {
            get { return (OpenCommand)this.GetValue(OpenCommandProperty); }
            set { this.SetValue(OpenCommandProperty, value); }
        }

        public CloseCommand CloseCommand
        {
            get { return (CloseCommand)this.GetValue(CloseCommandProperty); }
            set { this.SetValue(CloseCommandProperty, value); }
        }

        public SaveCommand SaveCommand
        {
            get { return (SaveCommand)this.GetValue(SaveCommandProperty); }
            set { this.SetValue(SaveCommandProperty, value); }
        }

        public NewCommand NewCommand
        {
            get { return (NewCommand)this.GetValue(NewCommandProperty); }
            set { this.SetValue(NewCommandProperty, value); }
        }

        private void RegisterCommands()
        {
            this.OpenCommand = new OpenCommand();
            this.CloseCommand = new CloseCommand();
            this.SaveCommand = new SaveCommand();
            this.NewCommand = new NewCommand();
        }
    }
}
