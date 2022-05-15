namespace PoshDotnetDumpAnalyzeViewer;

public record OutputLine(string Line) : IOutputLine
{
    public override string ToString() => Line;
}

public sealed record HelpOutputLine(string Line) : OutputLine(Line), IHelpCommand
{
    public override string ToString() => Line;
    public string[] Commands => Parser.Help.GetCommandsFromLine(Line);
}

public record struct DumpHeapRanges(Range Address, Range MethodTable, Range Size);

public sealed record DumpHeapOutputLine(string Line, DumpHeapRanges Ranges) : OutputLine(Line), IMethodTable, IAddress
{
    public override string ToString() => Line;
    public ReadOnlyMemory<char> MethodTable => Line.AsMemory(Ranges.MethodTable);
    public ReadOnlyMemory<char> Address => Line.AsMemory(Ranges.Address);
}

public record struct DumpHeapStatisticsRanges(Range MethodTable, Range Count, Range TotalSize, Range ClassName);

public sealed record DumpHeapStatisticsOutputLine(string Line, DumpHeapStatisticsRanges Ranges) : OutputLine(Line),
    IMethodTable, ITypeName
{
    public override string ToString() => Line;
    public ReadOnlyMemory<char> MethodTable => Line.AsMemory(Ranges.MethodTable);
    public ReadOnlyMemory<char> TypeName => Line.AsMemory(Ranges.ClassName);
}

public sealed record SetThreadOutputLine(string Line) : OutputLine(Line), IOsThreadId
{
    public override string ToString() => Line;
    public ReadOnlyMemory<char> OsThreadId => Parser.SetThread.GetOsIDFromSetThreadLine(Line);
}

public record struct ClrThreadsIndexes(Range Dbg, Range Id, Range OsId, Range ThreadObj, Range State, Range GcMode,
    Range GcAllocContext, Range Domain, Range LockCount, Range AptException); // not sure if AptException is one thing or 2 different ones

public sealed record ClrThreads(string Line) : OutputLine(Line), IOsThreadId
{
    public override string ToString() => Line;
    public ReadOnlyMemory<char> OsThreadId => Parser.SetThread.GetOsIDFromSetThreadLine(Line);
}