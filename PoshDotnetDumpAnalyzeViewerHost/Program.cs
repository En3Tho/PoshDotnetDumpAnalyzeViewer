using PoshDotnetDumpAnalyzeViewer;

if (args.Length == 0)
{
    Console.WriteLine("PoshDotnetDumpAnalyzeViewerHost <dump_file>");
    Console.WriteLine("Please specify path to dump");
    return;
}

try
{
    const string fileName = @"X:\Sources\dotnet\diagnostics\artifacts\bin\dotnet-dump\Release\net6.0\dotnet-dump.exe";
    //const string fileName = @"dotnet-dump";
    var analyzeArgs = args[0];
    await App.Run(fileName, analyzeArgs);
}
catch (Exception exn)
{
    Console.WriteLine(exn);
}