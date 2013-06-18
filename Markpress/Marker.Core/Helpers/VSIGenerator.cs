namespace MarkdownContent.Core.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using MarkdownContent.Core.Model;

    public class VSIGenerator
    {
        private string tempFolder;

        public void CleanTempFiles()
        {
            if (!Directory.Exists(this.tempFolder))
            {
                return;
            }

            string[] files = Directory.GetFiles(this.tempFolder);

            foreach (string f in files)
            {
                File.Delete(f);
            }

            Directory.Delete(this.tempFolder);
        }

        public void GenerateTempFolders(string vsiPath)
        {
            StepNotificationHelper.Initialize(2);
            StepNotificationHelper.Step("Generating Temporal Folders");
            this.tempFolder = Path.GetDirectoryName(vsiPath) + "\\vsitemp\\";
            if (Directory.Exists(this.tempFolder))
            {
                Directory.Delete(this.tempFolder, true);
            }

            Directory.CreateDirectory(this.tempFolder);
            StepNotificationHelper.Step();
        }

        public void GenerateVSI(string vsiPath, List<CodeSnippet> snippets)
        {
            // Adding two steps for Compressing files task + Generating snippet files task
            StepNotificationHelper.Initialize(snippets.Count() + 2);

            this.CreateTempFiles(snippets, this.tempFolder, vsiPath);

            if (File.Exists(vsiPath))
            {
                File.Delete(vsiPath);
            }

            StepNotificationHelper.Step("Compressing files...");
            this.ZipContent(this.tempFolder, vsiPath);
        }

        private void CreateTempFiles(List<CodeSnippet> snippets, string tempFolder, string vsiPath)
        {
            string destPath = null;
            string installerPath = null;
            string snippetContent = null;
            string installerContentManifest = null;

            StepNotificationHelper.Step("Generating .snippet files...");

            installerContentManifest = "<VSContent xmlns=\"http://schemas.microsoft.com/developer/vscontent/2005\">";

            foreach (CodeSnippet s in snippets)
            {
                StepNotificationHelper.Step();
                destPath = Path.Combine(tempFolder, s.Filename + ".snippet");

                installerContentManifest += s.GetVSContent();
                snippetContent = s.GetSnippetContent();

                this.WriteToFile(snippetContent, destPath);
            }

            installerContentManifest += "</VSContent>";
            installerPath = Path.Combine(tempFolder, Path.GetFileNameWithoutExtension(vsiPath) + ".vscontent");
            this.WriteToFile(installerContentManifest, installerPath);
        }

        private void WriteToFile(string content, string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    throw new System.IO.IOException("Code Snippet already exists.");
                }

                StreamWriter writer = new StreamWriter(path);
                writer.Write(content);
                writer.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while trying to write a code snippet into disk:" + ex.Message, ex);
            }
        }

        private void ZipContent(string folder, string zipFilename)
        {
            ZipHelper.ZipFolder(folder, zipFilename);
        }
    }
}
