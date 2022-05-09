using PoshDotnetDumpAnalyzeViewer;

try
{
    await App.Run(@"C:\Users\RZRL\dump_20220506_140504.dmp");
}
catch (Exception exn)
{
    Console.WriteLine(exn);
}