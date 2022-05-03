using PoshDotnetDumpAnalyzeViewer;

var dotnetDump = await ProcessUtil.StartDotnetDumpAnalyze(@"/home/oem/Source/Dumps/core_20220502_234252");

UI.Run(dotnetDump);

dotnetDump.Kill(true);