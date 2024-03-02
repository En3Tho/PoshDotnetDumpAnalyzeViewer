namespace PoshDotnetDumpAnalyzeViewer.Parsing;

public interface IClrThreadId
{
    string ClrThreadId { get; }
}

public interface IObjectAddress
{
    string Address { get; }
}

public interface IMethodTable
{
    string MethodTable { get; }
}

public interface ITypeName
{
    string TypeName { get; }
}

public interface IOsThreadId
{
    string OsThreadId { get; }
}

public interface ISyncBlockIndex
{
    string SyncBlockIndex { get; }
}

public interface IThreadState
{
    string ThreadState { get; }
}

public interface ISyncBlockAddress
{
    string SyncBlockAddress { get; }
}

public interface ISyncBlockOwnerAddress
{
    string SyncBlockOwnerAddress { get; }
}

public interface ISyncBlockOwnerTypeName
{
    string SyncBlockOwnerTypeName { get; }
}

public interface IEEClassAddress
{
    string EEClassAddress { get; }
}

public interface IModuleAddress
{
    string ModuleAddress { get; }
}

public interface IAssemblyAddress
{
    string AssemblyAddress { get; }
}

public interface IDomainAddress
{
    string DomainAddress { get; }
}

public record OutputLine(string Line)
{
    public sealed override string ToString() => Line;
}

public record struct ObjectAddressRanges(Range Address);

public sealed record ObjectAddressOutputLine(string Line, string Address)
    : OutputLine(Line), IObjectAddress;

public interface IExceptionObjectAddress : IObjectAddress;
public sealed record ExceptionObjectAddressOutputLine(string Line, string Address)
    : OutputLine(Line), IExceptionObjectAddress;

public record struct TypeNameRanges(Range TypeName);

public sealed record TypeNameOutputLine(string Line, string TypeName)
    : OutputLine(Line), ITypeName;

public record struct MethodTableRanges(Range MethodTable);

public sealed record MethodTableOutputLine(string Line, string MethodTable)
    : OutputLine(Line), IMethodTable;

public record struct EEClassAddressRanges(Range EEClass);

public sealed record EEClassAddressOutputLine(string Line, string EEClassAddress)
    : OutputLine(Line), IEEClassAddress;

public record struct ModuleAddressRanges(Range Module);

public sealed record ModuleAddressOutputLine(string Line, string ModuleAddress)
    : OutputLine(Line), IModuleAddress;

public record struct AssemblyAddressRanges(Range Assembly);

public sealed record AssemblyAddressOutputLine(string Line, string AssemblyAddress)
    : OutputLine(Line), IAssemblyAddress;

public record struct DomainAddressRanges(Range Domain);

public sealed record DomainAddressOutputLine(string Line, string DomainAddress)
    : OutputLine(Line), IDomainAddress;

public record struct OsThreadIdRanges(Range OsThreadId);

public sealed record OsThreadIdOutputLine(string Line, string OsThreadId)
    : OutputLine(Line), IOsThreadId;

public sealed record HelpOutputLine(string Line, string[] Commands)
    : OutputLine(Line);

public record struct DumpObjectRanges(Range MethodTable, Range Field, Range Offset, Range Type, Range VT, Range Attr, Range Value, Range Name);

public sealed record DumpObjectOutputLine(string Line, string Address, string MethodTable)
    : OutputLine(Line), IMethodTable, IObjectAddress;

public record struct GCRootRanges(Range Address, Range TypeName);
public sealed record GCRootOutputLine(string Line, string Address, string TypeName)
    : OutputLine(Line), IObjectAddress, ITypeName;

public record struct DumpExceptionRanges(Range Address, Range MethodTable, Range TypeName);
public sealed record DumpExceptionsOutputLine(string Line, string Address, string MethodTable, string TypeName)
    : OutputLine(Line), IExceptionObjectAddress, IMethodTable, ITypeName;

public record struct DumpHeapRanges(Range Address, Range MethodTable, Range Size);

public sealed record DumpHeapOutputLine(string Line, string Address, string MethodTable)
    : OutputLine(Line), IMethodTable, IObjectAddress;

public record struct DumpHeapStatisticsRanges(Range MethodTable, Range Count, Range TotalSize, Range ClassName);

public sealed record DumpHeapStatisticsOutputLine(string Line, string MethodTable, string TypeName)
    : OutputLine(Line), IMethodTable, ITypeName;

public record struct ObjSizeRanges(Range Address, Range MethodTable, Range Size);

public sealed record ObjSizeOutputLine(string Line, string Address, string MethodTable)
    : OutputLine(Line), IMethodTable, IObjectAddress;

public record struct ObjSizeStatisticsRanges(Range MethodTable, Range Count, Range TotalSize, Range ClassName);

public sealed record ObjSizeStatisticsOutputLine(string Line, string MethodTable, string TypeName)
    : OutputLine(Line), IMethodTable, ITypeName;

public record struct ClrThreadsRanges(Range Dbg, Range Id, Range OsId, Range ThreadObj, Range State, Range GcMode,
    Range GcAllocContext, Range Domain, Range LockCount, Range Apt, Range Exception); // not sure if AptException is one thing or 2 different ones

public sealed record ClrThreadsOutputLine(string Line, string OsThreadId, string ClrThreadId, string ThreadState)
    : OutputLine(Line), IOsThreadId, IClrThreadId, IThreadState;

public record struct SyncBlockRanges(Range Index, Range SyncBlock, Range MonitorHeld, Range Recursion, Range OwningThreadAddress, Range OwningThreadOsId,
    Range OwningThreadDbgId, Range SyncBlockOwnerAddress, Range SyncBlockOwnerType);

public sealed record SyncBlockOutputLine(string Line, string OsThreadId, string SyncBlockOwnerAddress, string SyncBlockOwnerTypeName, string SyncBlockAddress, string SyncBlockIndex)
    : OutputLine(Line), ISyncBlockAddress, ISyncBlockIndex, IOsThreadId, ISyncBlockOwnerAddress, ISyncBlockOwnerTypeName;

public record struct SyncBlockZeroRanges(Range Index, Range SyncBlock, Range MonitorHeld, Range Recursion, Range OwningThreadAddress,
    Range OwningThreadDbgId, Range SyncBlockOwnerAddress, Range SyncBlockOwnerType);

public sealed record SyncBlockZeroOutputLine(string Line, string SyncBlockOwnerAddress, string SyncBlockOwnerTypeName, string SyncBlockAddress, string SyncBlockIndex)
    : OutputLine(Line), ISyncBlockAddress, ISyncBlockIndex, ISyncBlockOwnerAddress, ISyncBlockOwnerTypeName;

public record struct ParallelStacksRanges(Range OsThreadIds);

public sealed record ParallelStacksOutputLine(string Line, string[] OsThreadIds)
    : OutputLine(Line);