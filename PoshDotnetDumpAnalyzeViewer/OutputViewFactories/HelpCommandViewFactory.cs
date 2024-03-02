using System.Collections.Immutable;
using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using PoshDotnetDumpAnalyzeViewer.ViewBehavior;
using PoshDotnetDumpAnalyzeViewer.Views;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.OutputViewFactories;

public sealed record HelpCommandViewFactory
    (IClipboard Clipboard, CommandQueue CommandQueue, MainLayout MainLayout) : CommandViewFactoryBase(Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.Help);

    public override bool IsSupported(string command) =>
        command.Equals(Commands.Help, StringComparison.OrdinalIgnoreCase);

    protected override View CreateView(CommandOutput output)
    {
        var view = new CommandOutputView(output.Lines).AddDefaultBehavior(Clipboard);
        view.ListView.KeyDown += (_, args) =>
        {
            if (args.KeyCode == KeyCode.Enter)
            {
                if (view.ListView.TryParseLine<HelpParser>(output.Command) is HelpOutputLine help)
                {
                    CommandQueue.SendCommand($"help {help.Commands[0]}");
                    args.Handled = true;
                }
            }
        };

        return view;
    }
}