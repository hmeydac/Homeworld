namespace MarkdownContent.Core.Helpers
{
    using System.Collections.Generic;
    using System.IO;
    using Ionic.Zip;

    public static class ZipHelper
    {
        public static void ZipFolder(string folderToZip, string zipFileName)
        {
            using (ZipFile zip = new ZipFile(zipFileName))
            {
                zip.AddDirectory(folderToZip);

                zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
                zip.Save();
            }
        }

        public static void ZipFile(string fileToZip, string zipFileName)
        {
            using (ZipFile zip = new ZipFile(zipFileName))
            {
                zip.AddFile(fileToZip);

                zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
                zip.Save();
            }
        }

        public static void ZipFile(string fileToZip, string zipFileName, string relativePath)
        {
            using (ZipFile zip = new ZipFile(zipFileName))
            {
                zip.AddFile(fileToZip, relativePath);

                zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
                zip.Save();
            }
        }

        public static void Unzip(string fileToUnzip, string targetDirectory)
        {
            using (ZipFile zip = Ionic.Zip.ZipFile.Read(fileToUnzip))
            {
                foreach (ZipEntry e in zip)
                {
                    e.Extract(targetDirectory);
                }
            }
        }
    }
}
