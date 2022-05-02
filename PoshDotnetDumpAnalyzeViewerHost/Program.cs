using PoshDotnetDumpAnalyzeViewer;

var dotnetDump = await ProcessUtil.StartDotnetDumpAnalyze(@"D:\Downloads\ctdump\cloud-ru-29-04.dump");

UI.Run(dotnetDump);

dotnetDump.Kill(true);