namespace PoshDotnetDumpAnalyzeViewer;

public record OutputLine(string Line) : IOutputLine
{
    public sealed override string ToString() => Line;
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

public record struct EEClassAddressRanges(Range EEClass);

public sealed record EEClassAddressOutputLine(string Line, EEClassAddressRanges Ranges) : OutputLine(Line), IEEClassAddress
{
    public ReadOnlyMemory<char> EEClassAddress => Line.AsMemory(Ranges.EEClass);
}

public record struct ModuleAddressRanges(Range Module);

public sealed record ModuleAddressOutputLine(string Line, ModuleAddressRanges Ranges) : OutputLine(Line), IModuleAddress
{
    public ReadOnlyMemory<char> ModuleAddress => Line.AsMemory(Ranges.Module);
}

public record struct AssemblyAddressRanges(Range Assembly);

public sealed record AssemblyAddressOutputLine(string Line, AssemblyAddressRanges Ranges) : OutputLine(Line), IAssemblyAddress
{
    public ReadOnlyMemory<char> AssemblyAddress => Line.AsMemory(Ranges.Assembly);
}

public record struct DomainAddressRanges(Range Domain);

public sealed record DomainAddressOutputLine(string Line, DomainAddressRanges Ranges) : OutputLine(Line), IDomainAddress
{
    public ReadOnlyMemory<char> DomainAddress => Line.AsMemory(Ranges.Domain);
}

public sealed record HelpOutputLine(string Line) : OutputLine(Line), IHelpCommand
{
    public string[] Commands => HelpParser.GetCommandsFromLine(Line);
}

public record struct DumpObjectRanges(Range MethodTable, Range Field, Range Offset, Range Type, Range VT, Range Attr, Range Value, Range Name);

public sealed record DumpObjectOutputLine(string Line, DumpObjectRanges Ranges) : OutputLine(Line), IMethodTable
{
    public ReadOnlyMemory<char> MethodTable => Line.AsMemory(Ranges.MethodTable);
}

public record struct GCRootRanges(Range Address, Range TypeName);
public sealed record GCRootOutputLine(string Line, GCRootRanges Ranges) : OutputLine(Line), IAddress, ITypeName
{
    public ReadOnlyMemory<char> TypeName => Line.AsMemory(Ranges.TypeName);
    public ReadOnlyMemory<char> Address => Line.AsMemory(Ranges.Address);
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

public record struct ObjSizeRanges(Range Address, Range MethodTable, Range Size);

public sealed record ObjSizeOutputLine(string Line, ObjSizeRanges Ranges) : OutputLine(Line), IMethodTable, IAddress
{
    public ReadOnlyMemory<char> MethodTable => Line.AsMemory(Ranges.MethodTable);
    public ReadOnlyMemory<char> Address => Line.AsMemory(Ranges.Address);
}

public record struct ObjSizeStatisticsRanges(Range MethodTable, Range Count, Range TotalSize, Range ClassName);

public sealed record ObjSizeStatisticsOutputLine(string Line, ObjSizeStatisticsRanges Ranges) : OutputLine(Line),
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

public sealed record SyncBlockOutputLine(string Line, SyncBlockRanges Ranges) : OutputLine(Line), ISyncBlockAddress, ISyncBlockIndex, IOsThreadId, ISyncBlockOwnerAddress, ISyncBlockOwnerTypeName
{
    public ReadOnlyMemory<char> SyncBlockAddress => Line.AsMemory(Ranges.SyncBlock);
    public ReadOnlyMemory<char> SyncBlockIndex => Line.AsMemory(Ranges.Index);
    public ReadOnlyMemory<char> OsThreadId => Line.AsMemory(Ranges.OwningThreadOsId);
    public ReadOnlyMemory<char> SyncBlockOwnerAddress => Line.AsMemory(Ranges.SyncBlockOwnerAddress);
    public ReadOnlyMemory<char> SyncBlockOwnerTypeName => Line.AsMemory(Ranges.SyncBlockOwnerType);
}

public record struct SyncBlockZeroRanges(Range Index, Range SyncBlock, Range MonitorHeld, Range Recursion, Range OwningThreadAddress,
    Range OwningThreadDbgId, Range SyncBlockOwnerAddress, Range SyncBlockOwnerType);

public sealed record SyncBlockZeroOutputLine(string Line, SyncBlockZeroRanges Ranges) : OutputLine(Line), ISyncBlockAddress, ISyncBlockIndex, ISyncBlockOwnerAddress, ISyncBlockOwnerTypeName
{
    public ReadOnlyMemory<char> SyncBlockAddress => Line.AsMemory(Ranges.SyncBlock);
    public ReadOnlyMemory<char> SyncBlockIndex => Line.AsMemory(Ranges.Index);
    public ReadOnlyMemory<char> SyncBlockOwnerAddress => Line.AsMemory(Ranges.SyncBlockOwnerAddress);
    public ReadOnlyMemory<char> SyncBlockOwnerTypeName => Line.AsMemory(Ranges.SyncBlockOwnerType);
}