namespace MarkdownContent.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MarkdownContent.Core.Helpers;

    public class CodeSnippetProcessor
    {
        public void Run(SnippetsHelper helper)
        {
            var errorMessage = string.Empty;
            VSIGenerator generator = null;
            try
            {
                generator = new VSIGenerator();
                var codeSnippets = helper.GetCodeSnippets();                
                generator.GenerateTempFolders(helper.VSIPath);
                generator.GenerateVSI(helper.VSIPath, codeSnippets);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                throw ex;
            }
            finally
            {
                if (generator != null)
                {
                    generator.CleanTempFiles();
                }

                this.NotifyWorkCompleted(errorMessage);
            }
        }

        private void NotifyWorkCompleted(string errorMessage)
        {
            StepNotificationHelper.Initialize(1);
            if (errorMessage != string.Empty)
            {
                StepNotificationHelper.Step(errorMessage);
            }
            else
            {
                StepNotificationHelper.Step("Done!");
            }
        }
    }
}
