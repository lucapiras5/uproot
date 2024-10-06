// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Xml.Linq;
using uproot;

UprootOperation op;
if (!File.Exists("uproot.xml"))
{
    Utilities.WriteDefaultConfigFile();
    goto exit;
}

var doc = XDocument.Load("uproot.xml");
var to = doc.Descendants("to").FirstOrDefault(new XElement("to", ""));

if (to.Value == "")
{
    Console.WriteLine("Must provide a value for <to>");
    goto exit;
}
op = new UprootOperation(to.Value);
foreach (var from in doc.Descendants("from"))
{
    op.AddSource(from.Value);
}
op.EnsureDestinationPathExists();

var stats = new CopyStats(op.sourceFiles.Count);
foreach (var sourceFile in op.sourceFiles)
{

    Console.WriteLine($"{Utilities.ConvertBytesToMiB(sourceFile.FileInfo.Length):F2} MiB  {sourceFile.FileInfo.Name}{sourceFile.CounterSuffix}");
    op.CopyToDestination(sourceFile);
    stats.Update(sourceFile.FileInfo.Length);
    Console.WriteLine($"\t{Utilities.ConvertBytesToMiB((long)stats.AverageSpeed):F2} MiB/s  " +
        $"ETA: {Utilities.HumanReadableTimeSpan(stats.ETA)}  " +
        $"{Math.Ceiling(stats.Progress * 100)}%  ({stats.CopiedFiles}/{stats.TotalFiles})");
}

exit:

Console.Write("Press q to exit.");
ConsoleKeyInfo key;
do { key = Console.ReadKey(true); }
while (key.KeyChar != 'q');

