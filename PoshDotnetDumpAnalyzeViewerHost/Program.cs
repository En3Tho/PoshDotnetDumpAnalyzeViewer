using PoshDotnetDumpAnalyzeViewer;

try
{
    var dotnetDump = await ProcessUtil.StartDotnetDumpAnalyze(@"C:\Users\RZRL\dump_20220506_140504.dmp");

    UI.Run(dotnetDump);

    dotnetDump.Kill(true);
}
catch (Exception exn)
{
    Console.WriteLine(exn);
}