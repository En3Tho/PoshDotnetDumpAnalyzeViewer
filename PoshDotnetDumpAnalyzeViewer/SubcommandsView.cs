using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public interface IOutputLine<T>
{
    string Line { get; }
    static abstract T FromLine(string line);
}

public interface IThreadId
{
    Memory<char> TheadId { get; }
}

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

public static class SubcommandsView
{
    public static ListView ForOutputLine<T>(IOutputLine<T> line)
    {
        var view = new ListView(); // to popup

        if (line is IAddress address)
        {
            // helpers ...
        }

        if (line is IMethodTable methodTable)
        {
            // helpers ...
        }

        if (line is ITypeName typeName)
        {
            // helpers ...
        }

        if (line is IThreadId threadId)
        {
            // helpers ...
        }

        return view;
    }
}