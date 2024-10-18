using System.Collections.Immutable;
using PoshDotnetDumpAnalyzeViewer.UI.Behavior;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.UI.OutputViewFactories;

public sealed record SosCommandViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue, CommandViewFactoryBase[] Factories) : CommandViewFactoryBase(Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } = [Commands.Sos, Commands.Ext];

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