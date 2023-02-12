namespace PoshDotnetDumpAnalyzeViewer;

public record OutputLine(string Line) : IOutputLine // IParsable<OutputLine> ?
{
    public override string ToString() => Line;
}

public sealed record HelpOutputLine(string Line) : OutputLine(Line), IHelpCommand
{
    public override string ToString() => Line;
    public string[] Commands => Help.GetCommandsFromLine(Line);
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

public record struct SetThreadRanges(Range OsThreadId);

public sealed record SetThreadOutputLine(string Line, SetThreadRanges Ranges) : OutputLine(Line), IOsThreadId
{
    public override string ToString() => Line;
    public ReadOnlyMemory<char> OsThreadId => Line.AsMemory(Ranges.OsThreadId);
}

public record struct ClrThreadsRanges(Range Dbg, Range Id, Range OsId, Range ThreadObj, Range State, Range GcMode,
    Range GcAllocContext, Range Domain, Range LockCount, Range Apt, Range Exception); // not sure if AptException is one thing or 2 different ones

public sealed record ClrThreadsOutputLine(string Line, ClrThreadsRanges Ranges) : OutputLine(Line), IOsThreadId, IClrThreadId, IThreadState
{
    public override string ToString() => Line;
    public ReadOnlyMemory<char> OsThreadId => Line.AsMemory(Ranges.OsId);
    public ReadOnlyMemory<char> ClrThreadId => Line.AsMemory(Ranges.Id);
    public ReadOnlyMemory<char> ThreadState => Line.AsMemory(Ranges.State);
}

public record struct SyncBlockRanges(Range Index, Range SyncBlock, Range MonitorHeld, Range Recursion, Range OwningThreadAddress, Range OwningThreadOsId,
    Range OwningThreadDbgId, Range SyncBlockOwnerAddress, Range SyncBlockOwnerType);

public sealed record SyncBlockOutputLine(string Line, SyncBlockRanges Ranges) : OutputLine(Line), IOsThreadId, ISyncBlockOwnerAddress
{
    public override string ToString() => Line;
    public ReadOnlyMemory<char> OsThreadId => Line.AsMemory(Ranges.OwningThreadOsId);
    public ReadOnlyMemory<char> SyncBlockOwnerAddress => Line.AsMemory(Ranges.SyncBlockOwnerAddress);
}