using System.Collections.Immutable;
using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using PoshDotnetDumpAnalyzeViewer.ViewBehavior;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.Views;

public sealed record DefaultCommandOutputViewFactory(IClipboard Clipboard) : CommandOutputViewFactoryBase(Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } = [];
    public override bool IsSupported(string command) => true;

    protected override View CreateView(CommandOutput output)
    {
        var views = new CommandOutputView(output.Lines).AddDefaultBehavior(Clipboard);
        return views;
    }
}

public sealed record QuitCommandOutputViewFactory(IClipboard Clipboard) : CommandOutputViewFactoryBase(Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } =
        ImmutableArray.Create(Commands.Exit, Commands.Q, Commands.Quit);

    public override bool IsSupported(string command) =>
        SupportedCommands.Any(supportedCommand => command.Equals(supportedCommand, StringComparison.OrdinalIgnoreCase));

    protected override View CreateView(CommandOutput output)
    {
        Environment.Exit(0);
        return null;
    }
}

public sealed record HelpCommandOutputViewFactory
    (IClipboard Clipboard, CommandQueue CommandQueue, MainLayout MainLayout) : CommandOutputViewFactoryBase(Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.Help);

    public override bool IsSupported(string command) =>
        command.Equals(Commands.Help, StringComparison.OrdinalIgnoreCase);

    protected override View CreateView(CommandOutput output)
    {
        var views = new CommandOutputView(output.Lines).AddDefaultBehavior(Clipboard);
        views.ListView.KeyDown += (_, args) =>
        {
            if (args.KeyCode == KeyCode.Enter)
            {
                if (views.ListView.TryParseLine<Parsing.HelpParser>(Commands.Help) is HelpOutputLine line)
                {
                    var command = line.Commands[0];
                    CommandQueue.SendCommand($"help {command}");
                    args.Handled = true;
                }
            }
        };

        return views;
    }
}

public sealed record DumpHeapCommandOutputViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<Parsing.DumpHeapParser>(
        MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpHeap);
}

public sealed record ObjSizeCommandOutputViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<Parsing.ObjSizeParser>(
        MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.ObjSize);
}

public sealed record SetThreadCommandOutputViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<Parsing.SetThreadParser>(
        MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.SetThread, Commands.Threads);
    public override string NormalizeCommand(string command)
    {
        var start = command[0] == 's' ? Commands.SetThread.Length : Commands.Threads.Length;
        var span = command.AsSpan(start).Trim();

        if (span.StartsWith("-t"))
        {
            try
            {
                var threadId = OsThreadIdReader.Read(span[2..]);
                return $"{(command[0] == 's' ? Commands.SetThread : Commands.Threads)} -t {threadId}";
            }
            catch
            {
                return base.NormalizeCommand(command);
            }
        }

        return base.NormalizeCommand(command);
    }
}

public sealed record ClrThreadsCommandOutputViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<Parsing.ClrThreadsParser>(
        MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.ClrThreads);
}

public sealed record SyncBlockCommandOutputViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<Parsing.SyncBlockParser>(
        MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.SyncBlock);
}

public sealed record DumpObjectCommandOutputViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<Parsing.DumpObjectParser>(
        MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpObject);
}

public sealed record DumpMethodTableCommandOutputViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<Parsing.DumpMethodTableParser>(
        MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpMethodTable);
}

public sealed record DumpModuleCommandOutputViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<Parsing.DumpModuleParser>(
        MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpModule);
}

public sealed record DumpClassCommandOutputViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<Parsing.DumpClassParser>(
        MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpClass);
}

public sealed record DumpDomainCommandOutputViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<Parsing.DumpDomainParser>(
        MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpDomain);
}

public sealed record DumpAssemblyCommandOutputViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<Parsing.DumpAssemblyParser>(
        MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpAssembly);
}

public sealed record Name2EECommandOutputViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<Parsing.Name2EEParser>(
        MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.Name2EE);
}

public sealed record GCRootCommandOutputViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<Parsing.GCRootParser>(
        MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.GCRoot);
}

public sealed record PrintExceptionOutputFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<Parsing.PrintExceptionParser>(
        MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.PrintException);
}

public sealed record DumpExceptionOutputFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<Parsing.DumpExceptionsParser>(
        MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpExceptions);
}

public sealed record ParallelStacksOutputFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<Parsing.ParallelStacksParser>(
        MainLayout, Clipboard, CommandQueue)
{
    public static string[] ShrinkParallelStacksOutput(string[] lines)
    {
        var reducedStack = new List<string>(lines.Length / 10);
        var currentThreadCount = -1;
        var firstLine = "";
        var lastLine = "";

        foreach (var line in lines)
        {
            if (Parsing.ParallelStacksParser.IsThreadNames(line))
            {
                reducedStack.Add(firstLine);
                reducedStack.Add(lastLine);
                reducedStack.Add(line);
                currentThreadCount = -1;
                continue;
            }

            if (Parsing.ParallelStacksParser.TryParseThreadCount(line, out var threadCount))
            {
                if (currentThreadCount != -1 && currentThreadCount != threadCount)
                {
                    reducedStack.Add(firstLine);
                    if (!ReferenceEquals(firstLine, lastLine))
                    {
                        reducedStack.Add(lastLine);
                    }
                }

                if (currentThreadCount != threadCount)
                {
                    firstLine = line;
                    currentThreadCount = threadCount;
                }

                lastLine = line;
                continue;
            }

            reducedStack.Add(line);
        }

        return reducedStack.ToArray();
    }

    // special case pstacks for this viewer
    protected override View CreateView(CommandOutput output)
    {
        var lines = output.Lines;
        lines.AsSpan(1).Reverse();
        return base.CreateView(output);
    }

    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.ParallelStacks);
}

public sealed record ClrStackOutputViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<Parsing.ClrStackParser>(
        MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.ClrStack);
}

public sealed record SosCommandOutputViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue, CommandOutputViewFactoryBase[] Factories) : CommandOutputViewFactoryBase(Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.Sos, Commands.Ext);

    protected override View CreateView(CommandOutput output)
    {
        var trimmedCommand = output.Command.TrimStart('s', 'o', 's', 'e', 'x', 't', ' ');
        if (Factories.FirstOrDefault(x => x.IsSupported(trimmedCommand)) is {} factory)
        {
            var trimmedOutput = output with { Command = trimmedCommand };
            return factory.HandleOutput(trimmedOutput);
        }

        return new CommandOutputView(output.Lines).AddDefaultBehavior(Clipboard);
    }
}