namespace MarkdownContent.Core.Helpers
{
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class DirectoryHelper
    {
        public static void CopyDirectory(string sourceDirectory, string destinationDirectory)
        {
            CopyDirectory(sourceDirectory, destinationDirectory, null, null);
        }

        public static void CopyDirectory(string sourceDirectory, string destinationDirectory, string[] excludePatterns, string[] includePatterns)
        {
            if (sourceDirectory == destinationDirectory)
            {
                return;
            }

            if (excludePatterns == null)
            {
                excludePatterns = new string[] { };
            }

            var contentFiles = Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories).ToList();
            var currentCount = 1;
            var total = contentFiles.Count;
            ProgressNotificationHelper.Clear();
            foreach (var fileName in contentFiles)
            {
                // Notifying progress
                ProgressNotificationHelper.ReportProgress(currentCount / total, "Processing Files");
                currentCount += 1;

                var fileInfo = new FileInfo(fileName);
                if (excludePatterns.Contains(fileInfo.Extension) ||
                    excludePatterns.Contains(fileInfo.Name) ||
                    excludePatterns.Any(mask => fileInfo.Name.MatchMask(mask)) ||
                    excludePatterns.Any(mask => fileInfo.FullName.MatchMask(mask)))
                {
                    continue;
                }

                // http://en.wikipedia.org/wiki/De_Morgan%27s_laws
                if (includePatterns != null &&
                    !includePatterns.Contains(fileInfo.Extension) &&
                    !includePatterns.Contains(fileInfo.Name) &&
                    !includePatterns.Any(mask => fileInfo.Name.MatchMask(mask)) &&
                    !includePatterns.Any(mask => fileInfo.FullName.MatchMask(mask)))
                {
                    continue;
                }

                var destinationFileInfo = GetDestinationFileInfo(sourceDirectory, destinationDirectory, fileInfo);
                CopyFile(fileInfo, destinationFileInfo);
            }
        }

        public static void CopyFile(string sourceFile, string destinationFile)
        {
            FileInfo originFileInfo = new FileInfo(sourceFile);
            FileInfo destinationFileInfo = new FileInfo(destinationFile);

            CopyFile(originFileInfo, destinationFileInfo);
        }

        private static bool MatchMask(this string fileName, string fileMask)
        {
            Regex mask = new Regex(fileMask.Replace(".", "[.]").Replace("*", ".*").Replace("?", "."));
            return mask.IsMatch(fileName);
        }

        private static void CopyFile(FileInfo originFileInfo, FileInfo destinationFileInfo)
        {
            if (originFileInfo.Exists)
            {
                if (!destinationFileInfo.Directory.Exists)
                {
                    destinationFileInfo.Directory.Create();
                }

                if (destinationFileInfo.Exists == false || (destinationFileInfo.Length != originFileInfo.Length))
                {
                    originFileInfo.CopyTo(destinationFileInfo.FullName, true);
                }
            }
        }

        private static FileInfo GetDestinationFileInfo(string sourceDirectory, string destinationDirectory, FileInfo fileInfo)
        {
            var subFolder = fileInfo.DirectoryName.Replace(sourceDirectory, string.Empty);
            if (!string.IsNullOrEmpty(subFolder))
            {
                subFolder = subFolder.Substring(1);
            }

            var destinationPath = Path.Combine(destinationDirectory, subFolder, fileInfo.Name);
            var destinationFileInfo = new FileInfo(destinationPath);
            return destinationFileInfo;
        }
    }
}
