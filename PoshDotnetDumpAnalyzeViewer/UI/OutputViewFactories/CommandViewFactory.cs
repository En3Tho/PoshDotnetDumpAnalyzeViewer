using System.Collections.Immutable;
using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.UI.Extensions;
using PoshDotnetDumpAnalyzeViewer.UI.Subcommands;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.UI.OutputViewFactories;

public abstract record CommandViewFactoryBase(IClipboard Clipboard)
{
    public abstract ImmutableArray<string> SupportedCommands { get; }

    public virtual bool IsSupported(string command) => SupportedCommands.Any(supportedCommand =>
        command.StartsWith(supportedCommand, StringComparison.OrdinalIgnoreCase));

    public virtual string NormalizeCommand(string command) => command;

    public View HandleOutput(CommandOutput output)
    {
        if (!IsSupported(output.Command))
            throw new NotSupportedException($"{GetType().FullName} does not support command {output.Command}");

        return CreateView(output);
    }

    protected abstract View CreateView(CommandOutput output);
}

public sealed record FallbackCommandViewFactory(IClipboard Clipboard) : CommandViewFactoryBase(Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } = [];
    public override bool IsSupported(string command) => true;

    protected override View CreateView(CommandOutput output)
    {
        var view = new CommandOutputView(output.Lines).AddDefaultBehavior(Clipboard);
        return view;
    }
}

public abstract record CommandViewFactory<TParser>(MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : CommandViewFactoryBase(Clipboard)
    where TParser : IOutputParser, new()
{
    protected override View CreateView(CommandOutput output)
    {
        var views =
            new CommandOutputView(output.Lines)
                .AddDefaultBehavior(Clipboard);

        views.ListView.HandleEnter(
            line =>
            {
                if (views.ListView.TryParseLine<TParser>(line) is { } outputLine)
                {
                    return SubcommandsDialog.TryCreate(MainLayout, outputLine, _ => [], Clipboard, CommandQueue);
                }

                return null;
            },
            ex =>
            {
                CommandQueue.ExceptionHandler(ex);
                return true;
            });

        return views;
    }
}

public sealed record ClrStackViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : CommandViewFactory<ClrStackParser>(
    MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.ClrStack);
}

public sealed record ClrThreadsCommandViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : CommandViewFactory<ClrThreadsParser>(
    MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.ClrThreads);
}

public sealed record DumpAssemblyCommandViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : CommandViewFactory<DumpAssemblyParser>(
    MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpAssembly);
}

public sealed record DumpClassCommandViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : CommandViewFactory<DumpClassParser>(
    MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpClass);
}

public sealed record DumpDomainCommandViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : CommandViewFactory<DumpDomainParser>(
    MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpDomain);
}

public sealed record DumpExceptionFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : CommandViewFactory<DumpExceptionsParser>(
    MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpExceptions);
}

public sealed record DumpHeapCommandViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : CommandViewFactory<DumpHeapParser>(
    MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpHeap);
}

public sealed record DumpMethodTableCommandViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : CommandViewFactory<DumpMethodTableParser>(
    MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpMethodTable);
}

public sealed record DumpModuleCommandViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : CommandViewFactory<DumpModuleParser>(
    MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpModule);
}

public sealed record DumpObjectCommandViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : CommandViewFactory<DumpObjectParser>(
    MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpObject);
}

public sealed record GcRootCommandViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : CommandViewFactory<GCRootParser>(
    MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.GCRoot);
}

public sealed record Name2EeCommandViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : CommandViewFactory<Name2EEParser>(
    MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.Name2EE);
}

public sealed record ObjSizeCommandViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : CommandViewFactory<ObjSizeParser>(
    MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.ObjSize);
}

public sealed record PrintExceptionFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : CommandViewFactory<PrintExceptionParser>(
    MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.PrintException);
}

public sealed record SetThreadCommandViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : CommandViewFactory<SetThreadParser>(
    MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.SetThread, Commands.Threads);
}

public sealed record SyncBlockCommandViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : CommandViewFactory<SyncBlockParser>(
    MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.SyncBlock);
}