namespace PoshDotnetDumpAnalyzeViewer.Subcommands;

public enum SubcommandsPriority
{
    Copy = 0,
    DumpHeap = 1,
    PrintException = 1,
    GcRoot = 2,
    DumpObject = 3,
    DumpMethodTable = 4,
    ParallelStacks = 5,
    SetThread = 5,
    ThreadState = 9,
    SyncBlock = 9,

    // let them have the lowest priority for now because there are lot of them
    DumpMemory = 100
}