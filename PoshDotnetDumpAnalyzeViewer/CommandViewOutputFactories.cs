using System.Collections.Immutable;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public struct DefaultOutputParser : IOutputParser
{
    public CommandOutput Parse(string command, string[] output)
    {
        return new(command, output.Map(x => new OutputLine(x)));
    }
}

public sealed record DefaultCommandOutputViewFactory(IClipboard Clipboard) : CommandOutputViewFactoryBase<DefaultOutputParser>(Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } = new();
    public override bool IsSupported(string command) => true;

    protected override CommandOutputViews CreateView(CommandOutput output)
    {
        var views = UI.MakeDefaultCommandViews().SetupLogic(Clipboard, output.Lines);
        return views;
    }
}

public sealed record QuitCommandOutputViewFactory(IClipboard Clipboard) : CommandOutputViewFactoryBase<DefaultOutputParser>(Clipboard)
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

public struct HelpOutputParser : IOutputParser
{
    public CommandOutput Parse(string command, string[] output)
        => Parser.Help.Parse(command, output);
}

public sealed record HelpCommandOutputViewFactory
    (IClipboard Clipboard, CommandQueue CommandQueue) : CommandOutputViewFactoryBase<HelpOutputParser>(Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.Help);

    public override bool IsSupported(string command) =>
        command.Equals(Commands.Help, StringComparison.OrdinalIgnoreCase);

    protected override CommandOutputViews CreateView(CommandOutput output)
    {
        var views = UI.MakeDefaultCommandViews().SetupLogic(Clipboard, output.Lines);

        views.OutputListView.KeyPress += args =>
        {
            // To function ?
            if (args.KeyEvent.Key == Key.Enter)
            {
                if (views.OutputListView.GetSelectedOutput<IHelpCommand>() is { } line)
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

public struct DumpHeapOutputParser : IOutputParser
{
    public CommandOutput Parse(string command, string[] output) =>
        Parser.DumpHeap.Parse(command, output);
}

public sealed record DumpHeapCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : DefaultViewsOutputViewFactoryBase<DumpHeapOutputParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpHeap);
}

public struct SetThreadOutputParser : IOutputParser
{
    public CommandOutput Parse(string command, string[] output) =>
        Parser.SetThread.Parse(command, output);
}

public sealed record SetThreadCommandOutputViewFactory
    (TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : DefaultViewsOutputViewFactoryBase<SetThreadOutputParser>(
        TopLevelViews, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.SetThread, Commands.Threads);
}