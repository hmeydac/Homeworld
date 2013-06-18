namespace MarkdownContent.Core.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class StepNotificationHelper
    {
        private static int total;
        private static int currentStep;

        public static void Initialize(int totalSteps)
        {
            total = totalSteps;
            currentStep = 0;
        }

        public static void Step()
        {
            Step(string.Empty);
        }

        public static void Step(string text)
        {
            currentStep++;
            decimal percentage = (decimal)currentStep / total;
            if (text == string.Empty)
            {
                ProgressNotificationHelper.ReportProgress(percentage);
            }
            else
            {
                ProgressNotificationHelper.ReportProgress(percentage, text);
            }
        }
    }
}
