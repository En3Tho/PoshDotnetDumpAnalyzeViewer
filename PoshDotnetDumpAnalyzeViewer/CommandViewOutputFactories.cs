using System.Collections.Immutable;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public sealed record DefaultCommandOutputViewFactory(IClipboard Clipboard) : CommandOutputViewFactoryBase(Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } = new();
    public override bool IsSupported(string command) => true;

    protected override CommandOutputViews CreateView(CommandOutput output)
    {
        var views = UI.MakeDefaultCommandViews(output).SetupLogic(Clipboard, output);
        return views;
    }
}

public sealed record QuitCommandOutputViewFactory(IClipboard Clipboard) : CommandOutputViewFactoryBase(Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } =
        ImmutableArray.Create(Commands.Exit, Commands.Q, Commands.Quit);

    public override bool IsSupported(string command) =>
        SupportedCommands.Any(supportedCommand => command.Equals(supportedCommand, StringComparison.OrdinalIgnoreCase));

    protected override CommandOutputViews CreateView(CommandOutput output)
    {
        Environment.Exit(0);
        return null;
    }
}

public sealed record HelpCommandOutputViewFactory
    (IClipboard Clipboard, CommandQueue CommandQueue, TopLevelViews TopLevelViews) : CommandOutputViewFactoryBase(Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.Help);

    public override bool IsSupported(string command) =>
        command.Equals(Commands.Help, StringComparison.OrdinalIgnoreCase);

    protected override CommandOutputViews CreateView(CommandOutput output)
    {
        var views = UI.MakeDefaultCommandViews(output).SetupLogic(Clipboard, output);

        views.OutputListView.KeyPress += args =>
        {
            if (args.KeyEvent.Key == Key.Enter)
            {
                if (views.OutputListView.TryParseLine<HelpParser>(Commands.Help) is HelpOutputLine line)
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
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<DumpHeapParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpHeap);
}

public sealed record ObjSizeCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<ObjSizeParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.ObjSize);
}

public sealed record SetThreadCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<SetThreadParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.SetThread, Commands.Threads);
}

public sealed record ClrThreadsCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<ClrThreadsParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.ClrThreads);
}

public sealed record SyncBlockCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<SyncBlockParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.SyncBlock);
}

public sealed record DumpObjectCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<DumpObjectParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpObject);
}

public sealed record DumpMethodTableCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<DumpMethodTableParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpMethodTable);
}

public sealed record DumpModuleCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<DumpModuleParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpModule);
}

public sealed record DumpClassCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<DumpClassParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpClass);
}

public sealed record DumpDomainCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<DumpDomainParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpDomain);
}

public sealed record DumpAssemblyCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<DumpAssemblyParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpAssembly);
}

public sealed record Name2EECommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<Name2EEParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.Name2EE);
}

public sealed record GCRootCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<GCRootParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.GCRoot);
}

public sealed record PrintExceptionOutputFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<PrintExceptionParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.PrintException);
}

public sealed record DumpExceptionOutputFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<DumpExceptionsParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpExceptions);
}

public sealed record ParallelStacksOutputFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<ParallelStacksParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public static string[] ShrinkParallelStacksOutput(string[] lines)
    {
        var reducedStack = new List<string>(lines.Length / 10);
        var currentThreadCount = -1;
        var firstLine = "";
        var lastLine = "";

        foreach (var line in lines)
        {
            if (ParallelStacksParser.IsThreadNames(line))
            {
                reducedStack.Add(firstLine);
                reducedStack.Add(lastLine);
                reducedStack.Add(line);
                currentThreadCount = -1;
                continue;
            }

            if (ParallelStacksParser.TryParseThreadCount(line, out var threadCount))
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
    protected override CommandOutputViews CreateView(CommandOutput output)
    {
        var lines = output.Lines;
        lines.AsSpan(1).Reverse();
        return base.CreateView(output);
    }

    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.ParallelStacks);
}

public sealed record ClrStackOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandOutputViewFactoryBase<ClrStackParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.ClrStack);
}

public sealed record SosCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue, ICommandOutputViewFactory[] Factories) : CommandOutputViewFactoryBase(Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.Sos, Commands.Ext);

    protected override CommandOutputViews CreateView(CommandOutput output)
    {
        var trimmedCommand = output.Command.TrimStart('s', 'o', 's', 'e', 'x', 't', ' ');
        return Factories.First(x => x.IsSupported(trimmedCommand)).HandleOutput(output);
    }
}