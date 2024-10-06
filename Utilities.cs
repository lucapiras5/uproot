using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uproot
{

    class CopyStats
    {
        private readonly Stopwatch stopwatch;
        public long CopiedBytes;
        public long CopiedFiles;
        public long TotalFiles;

        public CopyStats(int totalFiles)
        {
            this.TotalFiles = totalFiles;
            this.stopwatch = new Stopwatch();
            this.stopwatch.Start();
        }

        public void Update(long copiedBytes)
        {
            this.CopiedBytes += copiedBytes;
            this.CopiedFiles++;
        }

        private double ElapsedSeconds
        {
            get { return this.stopwatch.Elapsed.TotalSeconds; }
        }

        public double AverageSpeed
        {
            get { return CopiedBytes / this.ElapsedSeconds; }
        }

        public double Progress
        {
            get { return 1.0 * this.CopiedFiles / this.TotalFiles; }
        }

        public TimeSpan ETA
        {
            get
            {
                var averageTimePerFile = this.ElapsedSeconds / this.CopiedFiles;
                var filesLeft = this.TotalFiles - this.CopiedFiles;
                return TimeSpan.FromSeconds(averageTimePerFile * filesLeft);
            }
        }
    }

    internal static class Utilities

    {


        public static void WriteDefaultConfigFile()
        {
            Console.WriteLine("No uproot.xml configuration file found. Writing a sample one...");
            File.WriteAllText("uproot.xml", @"<uproot>
<from></from>
<to></to>
</uproot>
");
        }

        public static string HumanReadableTimeSpan(TimeSpan ts)
        {
            return $"{ts.Hours}h {ts.Minutes}m {ts.Seconds}s";
        }

        /// <summary>
        /// <para>Convert bytes into mebibytes (1 MiB = 1024^2 bytes).</para>
        /// <para>To be consistent with how Windows Explorer displays sizes, you can round down the value with <em>Math.floor()</em>.</para>
        /// <para><em>sizeInBytes</em> is <em>long</em> for compatibility with <em>FileInfo.Length</em>.</para>
        /// </summary>
        /// <returns>The size in MiB as a double.</returns>
        public static double ConvertBytesToMiB(long sizeInBytes)
        {
            return sizeInBytes / Math.Pow(1024, 2);
        }

        /// <summary>
        /// <para>Read a directory, and return the immediate sub-directories and files it contains.</para>
        /// </summary>
        public static (DirectoryInfo[], FileInfo[]) GetDirectoryContents(string path)
        {
            var di = new DirectoryInfo(path);
            return (di.GetDirectories(), di.GetFiles());
        }

        /// <summary>
        /// <para>Reads a directory recursively, extracts all the files and (by default) sorts them by name.</para>
        /// </summary>
        /// <param name="path"></param>
        /// <returns>A sorted List of FileInfo objects.</returns>
        public static List<FileInfo> GetFilesRecursively(string path, bool sort = true)
        {
            var filesFound = new List<FileInfo>();
            var directoriesToRead = new Queue<DirectoryInfo>();
            directoriesToRead.Enqueue(new DirectoryInfo(path));

            while (directoriesToRead.Count > 0)
            {
                var di = directoriesToRead.Dequeue();
                var (dirs, files) = Utilities.GetDirectoryContents(di.FullName);
                Array.ForEach(dirs, d => directoriesToRead.Enqueue(d));
                Array.ForEach(files, f => filesFound.Add(f));
            }
            return sort ?
                filesFound.OrderBy(f => f.FullName).ToList() :
                filesFound;          
        }
    }
}
