namespace Markpress.ViewModel
{
    public class MarkpressViewModel
    {
        public MarkpressViewModel()
        {
            this.RibbonContext = new RibbonViewModel();
            this.DocumentContext = new DocumentViewModel();
        }

        public RibbonViewModel RibbonContext { get; set; }

        public DocumentViewModel DocumentContext { get; set; }
    }
}
