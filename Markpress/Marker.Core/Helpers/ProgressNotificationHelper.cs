namespace MarkdownContent.Core.Helpers
{
    using System.ComponentModel;

    public class ProgressNotificationHelper
    {
        private static BackgroundWorker backgroundWorker;

        public static void Initialize(BackgroundWorker worker)
        {
            if (worker == null)
            {
                return;
            }

            backgroundWorker = worker;
            worker.ReportProgress(0);
        }

        public static void Clear()
        {
            if (backgroundWorker == null)
            {
                return;
            }

            backgroundWorker.ReportProgress(0);
        }

        public static void ReportProgress(decimal percentage, string message)
        {
            if (backgroundWorker == null)
            {
                return;
            }

            backgroundWorker.ReportProgress((int)(percentage * 100), message);
        }

        public static void ReportProgress(decimal percentage)
        {
            if (backgroundWorker == null)
            {
                return;
            }

            backgroundWorker.ReportProgress((int)(percentage * 100));
        }

        public static void Stop()
        {            
            backgroundWorker = null;
        }    
    }
}
