﻿using System.Collections.Immutable;
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
    (IClipboard Clipboard, CommandQueue CommandQueue) : CommandOutputViewFactoryBase(Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.Help);

    public override bool IsSupported(string command) =>
        command.Equals(Commands.Help, StringComparison.OrdinalIgnoreCase);

    protected override CommandOutputViews CreateView(CommandOutput output)
    {
        var views = UI.MakeDefaultCommandViews(output).SetupLogic(Clipboard, output);

        views.OutputListView.KeyPress += args =>
        {
            // To function ?
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
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : DefaultViewsOutputViewFactoryBase<DumpHeapParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpHeap);
}

public sealed record ObjSizeCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : DefaultViewsOutputViewFactoryBase<ObjSizeParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.ObjSize);
}

public sealed record SetThreadCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : DefaultViewsOutputViewFactoryBase<SetThreadParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.SetThread, Commands.Threads);
}

public sealed record ClrThreadsCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : DefaultViewsOutputViewFactoryBase<ClrThreadsParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.ClrThreads);
}

public sealed record SyncBlockCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : DefaultViewsOutputViewFactoryBase<SyncBlockParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.SyncBlock);
}

public sealed record DumpObjectCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : DefaultViewsOutputViewFactoryBase<DumpObjectParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpObject);
}

public sealed record DumpMethodTableCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : DefaultViewsOutputViewFactoryBase<DumpMethodTableParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpMethodTable);
}

public sealed record DumpModuleCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : DefaultViewsOutputViewFactoryBase<DumpModuleParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpModule);
}

public sealed record DumpClassCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : DefaultViewsOutputViewFactoryBase<DumpClassParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpClass);
}

public sealed record DumpDomainCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : DefaultViewsOutputViewFactoryBase<DumpDomainParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpDomain);
}

public sealed record DumpAssemblyCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : DefaultViewsOutputViewFactoryBase<DumpAssemblyParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpAssembly);
}

public sealed record Name2EECommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : DefaultViewsOutputViewFactoryBase<Name2EEParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.Name2EE);
}

public sealed record GCRootCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : DefaultViewsOutputViewFactoryBase<GCRootParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.GCRoot);
}

public sealed record PrintExceptionOutputFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : DefaultViewsOutputViewFactoryBase<PrintExceptionParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.PrintException);
}

public sealed record DumpExceptionOutputFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : DefaultViewsOutputViewFactoryBase<DumpExceptionsParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpExceptions);
}

public sealed record SosCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue, ICommandOutputViewFactory[] Factories) : CommandOutputViewFactoryBase(Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.Sos);

    protected override CommandOutputViews CreateView(CommandOutput output)
    {
        var trimmedCommand = output.Command.TrimStart('s', 'o', 's', 'e', 'x', 't', ' ');
        return Factories.First(x => x.IsSupported(trimmedCommand)).HandleOutput(output);
    }
}