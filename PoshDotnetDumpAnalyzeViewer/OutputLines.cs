namespace PoshDotnetDumpAnalyzeViewer;

public record OutputLine(string Line) : IOutputLine
{
    public sealed override string ToString() => Line;
}

public sealed record HelpOutputLine(string Line) : OutputLine(Line), IHelpCommand
{
    public string[] Commands => HelpParser.GetCommandsFromLine(Line);
}

public record struct ObjectAddressRanges(Range Address);

public sealed record ObjectAddressOutputLine(string Line, ObjectAddressRanges Ranges) : OutputLine(Line), IAddress
{
    public ReadOnlyMemory<char> Address => Line.AsMemory(Ranges.Address);
}

public record struct TypeNameRanges(Range TypeName);

public sealed record TypeNameOutputLine(string Line, TypeNameRanges Ranges) : OutputLine(Line), ITypeName
{
    public ReadOnlyMemory<char> TypeName => Line.AsMemory(Ranges.TypeName);
}

public record struct MethodTableRanges(Range MethodTable);

public sealed record MethodTableOutputLine(string Line, MethodTableRanges Ranges) : OutputLine(Line), IMethodTable
{
    public ReadOnlyMemory<char> MethodTable => Line.AsMemory(Ranges.MethodTable);
}

public record struct EEClassRanges(Range EEClass);

public sealed record EEClassOutputLine(string Line, EEClassRanges Ranges) : OutputLine(Line), IEEClassAddress
{
    public ReadOnlyMemory<char> EEClassAddress => Line.AsMemory(Ranges.EEClass);
}

public record struct DumpHeapRanges(Range Address, Range MethodTable, Range Size);

public sealed record DumpHeapOutputLine(string Line, DumpHeapRanges Ranges) : OutputLine(Line), IMethodTable, IAddress
{
    public ReadOnlyMemory<char> MethodTable => Line.AsMemory(Ranges.MethodTable);
    public ReadOnlyMemory<char> Address => Line.AsMemory(Ranges.Address);
}

public record struct DumpHeapStatisticsRanges(Range MethodTable, Range Count, Range TotalSize, Range ClassName);

public sealed record DumpHeapStatisticsOutputLine(string Line, DumpHeapStatisticsRanges Ranges) : OutputLine(Line),
    IMethodTable, ITypeName
{
    public ReadOnlyMemory<char> MethodTable => Line.AsMemory(Ranges.MethodTable);
    public ReadOnlyMemory<char> TypeName => Line.AsMemory(Ranges.ClassName);
}

public record struct SetThreadRanges(Range OsThreadId);

public sealed record SetThreadOutputLine(string Line, SetThreadRanges Ranges) : OutputLine(Line), IOsThreadId
{
    public ReadOnlyMemory<char> OsThreadId => Line.AsMemory(Ranges.OsThreadId);
}

public record struct ClrThreadsRanges(Range Dbg, Range Id, Range OsId, Range ThreadObj, Range State, Range GcMode,
    Range GcAllocContext, Range Domain, Range LockCount, Range Apt, Range Exception); // not sure if AptException is one thing or 2 different ones

public sealed record ClrThreadsOutputLine(string Line, ClrThreadsRanges Ranges) : OutputLine(Line), IOsThreadId, IClrThreadId, IThreadState
{
    public ReadOnlyMemory<char> OsThreadId => Line.AsMemory(Ranges.OsId);
    public ReadOnlyMemory<char> ClrThreadId => Line.AsMemory(Ranges.Id);
    public ReadOnlyMemory<char> ThreadState => Line.AsMemory(Ranges.State);
}

public record struct SyncBlockRanges(Range Index, Range SyncBlock, Range MonitorHeld, Range Recursion, Range OwningThreadAddress, Range OwningThreadOsId,
    Range OwningThreadDbgId, Range SyncBlockOwnerAddress, Range SyncBlockOwnerType);

public sealed record SyncBlockOutputLine(string Line, SyncBlockRanges Ranges) : OutputLine(Line), IOsThreadId, ISyncBlockOwnerAddress
{
    public ReadOnlyMemory<char> OsThreadId => Line.AsMemory(Ranges.OwningThreadOsId);
    public ReadOnlyMemory<char> SyncBlockOwnerAddress => Line.AsMemory(Ranges.SyncBlockOwnerAddress);
}