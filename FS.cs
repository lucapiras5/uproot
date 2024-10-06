using System.Diagnostics;

namespace uproot
{
    public struct FileInfoWithCounter
    {
        public FileInfo FileInfo;
        public int Counter;

        public readonly string CounterSuffix
        {
            get { return this.Counter > 1 ? $"-{this.Counter}{this.FileInfo.Extension}" : string.Empty; }
        }
    }

    class UprootOperation
    {

        private readonly string destinationDirectoryPath;
        public List<FileInfoWithCounter> sourceFiles;
        private readonly Dictionary<string, int> fileNameCounter;

        public UprootOperation(string destinationDirectoryPath)
        {
            var destinationExists = File.Exists(destinationDirectoryPath)
                || Directory.Exists(destinationDirectoryPath);
            if (destinationExists)
                throw new ArgumentException($"destination path already exists: {destinationDirectoryPath}");
            this.destinationDirectoryPath = destinationDirectoryPath;
            this.sourceFiles = [];
            this.fileNameCounter = [];
        }

        public void AddSource(string path)
        {
            foreach (var sourceFile in Utilities.GetFilesRecursively(path))
            {
                var sourceFileWithCounter = new FileInfoWithCounter { FileInfo = sourceFile };

                var fileName = sourceFile.Name;
                this.fileNameCounter[fileName] = this.fileNameCounter.ContainsKey(fileName) ?
                    ++this.fileNameCounter[fileName]
                    : 1;
                sourceFileWithCounter.Counter = this.fileNameCounter[fileName];

                this.sourceFiles.Add(sourceFileWithCounter);
            }
        }

        public void EnsureDestinationPathExists()
        {
            Directory.CreateDirectory(this.destinationDirectoryPath);
        }

        public void CopyToDestination(FileInfoWithCounter file)
        {
            var sourcePath = file.FileInfo.FullName;
            var destinationName = file.FileInfo.Name + file.CounterSuffix;
            var destinationPath = Path.Combine(this.destinationDirectoryPath, destinationName);

            File.Copy(sourcePath, destinationPath, true);
            var attrs = File.GetAttributes(sourcePath);
            File.SetAttributes(destinationPath, attrs);
        }
    }
}