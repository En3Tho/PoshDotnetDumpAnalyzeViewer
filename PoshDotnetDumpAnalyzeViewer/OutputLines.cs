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

public record struct DumpHeapIndexes(Range Address, Range MethodTable, Range Size);

public sealed record DumpHeapOutputLine(string Line, DumpHeapIndexes Indexes) : OutputLine(Line), IMethodTable, IAddress
{
    public override string ToString() => Line;
    public ReadOnlyMemory<char> MethodTable => Line.AsMemory(Indexes.MethodTable);
    public ReadOnlyMemory<char> Address => Line.AsMemory(Indexes.Address);
}

public record struct DumpHeapStatisticsIndexes(Range MethodTable, Range Count, Range TotalSize, Range ClassName);

public sealed record DumpHeapStatisticsOutputLine(string Line, DumpHeapStatisticsIndexes Indexes) : OutputLine(Line), IMethodTable, ITypeName
{
    public override string ToString() => Line;
    public ReadOnlyMemory<char> MethodTable => Line.AsMemory(Indexes.MethodTable);
    public ReadOnlyMemory<char> TypeName => Line.AsMemory(Indexes.ClassName);
}

public sealed record SetThreadOutputLine(string Line) : OutputLine(Line), IOsThreadId
{
    public override string ToString() => Line;
    public ReadOnlyMemory<char> OsThreadId => Parser.SetThread.GetOsIDFromSetThreadLine(Line);
}