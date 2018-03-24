using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FindDuplicates
{
    internal class FileClass
    {
        public FileClass(string fullPath, string fileNameWithExt, string fileNameOnly, string fileNameCleaned)
        {
            FullPath = fullPath;
            FileNameWithExt = fileNameWithExt;
            FileNameOnly = fileNameOnly;
            FileNameCleaned = fileNameCleaned;
        }

        public string FullPath { get; set; }
        public string FileNameWithExt { get; set; }
        public string FileNameOnly { get; set; }
        public string FileNameCleaned { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string[] mediaExtensions =
            {
                ".webm", ".mkv", ".flv", ".flv", ".vob", ".ogv", ".ogg", ".drc", ".gif", ".gifv", ".mng", ".avi",
                ".mov", ".qt", ".wmv", ".yuv", ".rm", ".rmvb", ".asf", ".amv", ".mp4", ".m4p", ".m4v", ".mpg", "mp2",
                ".mpeg", ".mpe", ".mpv", ".mpg", ".mpeg", ".m2v", ".m4v", ".svi", ".3gp", ".3g2", ".mxf", ".roq",
                ".nsv", ".flv", ".f4v", ".f4p", ".f4a", ".f4b", ".iso"
            };
            string[] charsToIgnore = {",", "!", " ", @"'"};
            var duplicateFileNames = new HashSet<string>();

            var logDir = new DirectoryInfo(Directory.GetCurrentDirectory());
            var startFolder = new DirectoryInfo(Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()));
            if (Debugger.IsAttached)
                //startFolder = new DirectoryInfo(AppSettingsReader.SourceFolder());
                startFolder = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos));
            string logFileName = Path.Combine(logDir.FullName, "log.txt");
            if (File.Exists(logFileName))
                File.Delete(logFileName);
            string isoDir = Path.Combine(Path.Combine(startFolder.FullName, "iso"));

            var fileClasses = new List<FileClass>();

            try
            {
                List<FileInfo> files = startFolder.GetFiles("*.*", SearchOption.AllDirectories).ToList();
                foreach (FileInfo fileInfo in files)
                {
                    if (fileInfo.DirectoryName.Equals(isoDir))
                        continue;
                    if (mediaExtensions.Contains(fileInfo.Extension, StringComparer.OrdinalIgnoreCase))
                    {
                        fileClasses.Add(new FileClass(fileInfo.FullName, Path.GetFileName(fileInfo.Name),
                            Path.GetFileNameWithoutExtension(fileInfo.Name),
                            Path.GetFileNameWithoutExtension(fileInfo.Name.Replace(charsToIgnore, ""))));
                    }
                }

                foreach (FileClass file in fileClasses)
                {
                    if (fileClasses.Count(x =>
                            x.FileNameCleaned.Equals(file.FileNameCleaned, StringComparison.OrdinalIgnoreCase)) > 1)
                    {
                        IEnumerable<FileClass> filteredFileClasses =
                            fileClasses.Where(x => x.FileNameCleaned == file.FileNameCleaned);
                        foreach (FileClass path in filteredFileClasses)
                        {
                            string fullPath = path.FullPath;
                            duplicateFileNames.Add(fullPath);
                            if (path.FullPath.Contains(".iso"))
                            {
                                if (!Directory.Exists(isoDir))
                                    Directory.CreateDirectory(isoDir);

                                if (File.Exists(fullPath) && !IsFileLocked(new FileInfo(fullPath)))
                                    File.Move(fullPath, Path.Combine(isoDir, $"{path.FileNameOnly}.iso"));
                            }
                        }
                    }
                }

                using (var writer = new StreamWriter(logFileName))
                {
                    Console.SetOut(writer);
                    foreach (string name in duplicateFileNames.ToList())
                        Console.WriteLine(name);
                }
            }
            catch (Exception e)
            {
                using (var writer = new StreamWriter(logFileName))
                {
                    Console.SetOut(writer);
                    Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                }
            }
        }

        private static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            if (!file.Exists) return false;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                stream?.Close();
            }

            //file is not locked
            return false;
        }
    }
}