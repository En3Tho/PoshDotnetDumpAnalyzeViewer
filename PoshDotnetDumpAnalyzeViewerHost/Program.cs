using PoshDotnetDumpAnalyzeViewer;

if (args.Length == 0)
{
    Console.WriteLine("PoshDotnetDumpAnalyzeViewerHost <dump_file>");
    Console.WriteLine("Please specify path to dump");
    return;
}

try
{
    await App.Run(args[0]);
}
catch (Exception exn)
{
    Console.WriteLine(exn);
}