using System.Collections.Immutable;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public sealed record DefaultCommandHandler(IClipboard Clipboard) : CommandHandler<DefaultOutputLine>(Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } = new();
    public override bool IsSupported(string command) => true;
    protected override Task<View> Run(CommandOutput<DefaultOutputLine> output)
    {
        var views = UI.MakeDefaultCommandViews(output.Command).SetupLogic(Clipboard, output.Lines);
        return Task.FromResult((View)views.Window);
    }
}

public sealed record QuitCommandHandler(IClipboard Clipboard) : CommandHandler<DefaultOutputLine>(Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } =
        new() { Commands.Exit, Commands.Q, Commands.Quit };

    public override bool IsSupported(string command) =>
        SupportedCommands.Any(supportedCommand => command.Equals(supportedCommand, StringComparison.OrdinalIgnoreCase));

    protected override Task<View> Run(CommandOutput<DefaultOutputLine> output)
    {
        Environment.Exit(0);
        return Task.FromResult<View>(null);
    }
}

public sealed record HelpCommandHandler(IClipboard Clipboard, CommandQueue CommandQueue) : CommandHandler<DefaultOutputLine>(Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } = new() { Commands.Help };

    public override bool IsSupported(string command) =>
        command.Equals(Commands.Help, StringComparison.OrdinalIgnoreCase);

    protected override Task<View> Run(CommandOutput<DefaultOutputLine> output)
    {
        var actualOutput = output.Lines.TakeAfter(new("Commands:"));
        var (window, listView, _) = UI.MakeDefaultCommandViews(output.Command).SetupLogic(Clipboard, actualOutput);

        listView.KeyPress += args =>
        {
            // To function ?
            if (args.KeyEvent.Key == Key.Enter)
            {
                if (listView.SelectedItem >= 0)
                {
                    var command = Parser.Help.GetCommandsFromLine(listView.GetSelectedItem<DefaultOutputLine>().ToString());
                    CommandQueue.SendCommand(command[0]);
                    args.Handled = true;
                }
            }
        };

        return Task.FromResult((View)window);
    }
}