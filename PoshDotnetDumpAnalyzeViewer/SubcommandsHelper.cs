namespace PoshDotnetDumpAnalyzeViewer;

public interface IAddress
{
    Memory<char> Address { get; }
}

public interface IMethodTable
{
    Memory<char> MethodTable { get; }
}

public interface ITypeName
{
    Memory<char> TypeName { get; }
}

public class SubcommandsHelper
{
    // TODO: subcommands
}