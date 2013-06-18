namespace Markpress.Processor
{
    using System;
    using System.IO;

    using Microsoft.Win32;

    public class DocumentHub
    {
        private static readonly string TemporalFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Markpress\";

        public static Document NewDocument()
        {
            var tempFolderName = Guid.NewGuid().ToString();
            var newDocumentFolder = Path.Combine(TemporalFolder, tempFolderName);
            try
            {
                Directory.CreateDirectory(newDocumentFolder);
                var document = new Document
                                   {
                                       FileName = string.Format("{0}.md", Guid.NewGuid()),
                                       FolderPath = newDocumentFolder,
                                       PreviewFileName = Path.Combine(newDocumentFolder, string.Format("{0}.html", Guid.NewGuid())),
                                   };

                var fileStream = new FileStream(document.FullFilename, FileMode.CreateNew, FileAccess.Write);
                fileStream.Close();
                return document;
            }
            catch (Exception ex)
            {
                throw new Exception("Could not create temporal document.", ex);
            }
        }

        public static bool SaveDocument(Document document)
        {
            if (document.IsTemporal)
            {
                var dialog = new SaveFileDialog { DefaultExt = "md", AddExtension = true };
                dialog.Filter = "Markdown files (*.md)|*.md|All files (*.*)|*.*";
                if (dialog.ShowDialog().Value)
                {
                    string destinationFolder = string.Empty;
                    destinationFolder = new FileInfo(dialog.FileName).DirectoryName;
                    try
                    {
                        var sourceFolder = new FileInfo(document.FullFilename).DirectoryName;
                        DirectoryCopy(sourceFolder, destinationFolder, true);
                        document.FolderPath = destinationFolder;
                        File.Move(document.FullFilename, Path.Combine(document.FolderPath, dialog.FileName));
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
            }

            document.HasChanges = false;
            return true;
        }

        private static void SaveMarkdownFile(Document document, string destinationFolder)
        {

        }

        private static void DirectoryCopy(string sourceDirectory, string destionationDirectory, bool copySubdirectories = false)
        {
            // Get the subdirectories for the specified directory.
            var directory = new DirectoryInfo(sourceDirectory);
            var subdirectories = directory.GetDirectories();

            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirectory);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destionationDirectory))
            {
                Directory.CreateDirectory(destionationDirectory);
            }

            // Get the files in the directory and copy them to the new location.
            var files = directory.GetFiles();
            foreach (var file in files)
            {
                string temppath = Path.Combine(destionationDirectory, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (!copySubdirectories)
            {
                return;
            }

            foreach (var subdir in subdirectories)
            {
                var temppath = Path.Combine(destionationDirectory, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, true);
            }
        }
    }
}
