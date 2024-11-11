using System.Collections.Immutable;
using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.UI.Behavior;
using PoshDotnetDumpAnalyzeViewer.UI.Subcommands;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.UI.OutputViewFactories;

public sealed record DumpArrayCommandViewFactory(MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : CommandViewFactory<DumpArrayParser>(
    MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = [ Commands.DumpArray ];

    protected override View CreateView(CommandOutput output)
    {
        var views =
            new CommandOutputView(output.Lines)
                .AddDefaultBehavior(Clipboard);

        string? elementMethodTable = null;
        foreach (var line in output.Lines)
        {
            if (DumpArrayParser.GetElementMethodTableRanges(line) is {} ranges)
            {
                elementMethodTable = line[ranges.MethodTable];
                break;
            }
        }

        if (elementMethodTable is null)
        {
            throw new InvalidOperationException("Unable to parse element method table from output");
        }

        views.ListView.HandleEnter(
            line =>
            {
                if (views.ListView.TryParseLine<DumpArrayParser>(line) is {} outputLine)
                {
                    Func<SubcommandButtonFactory, IEnumerable<SubcommandButton>> subcommandsFactory = factory =>
                    {
                        if (outputLine is IObjectAddress address)
                        {
                            return [ factory.MakeCommandButton(SubcommandsPriority.DumpMemory, "Dump value class", $"{Commands.DumpValueType} {elementMethodTable} {address.Address}") ];
                        }

                        return [ ];
                    };
                    return SubcommandsDialog.TryCreate(MainLayout, outputLine, subcommandsFactory, Clipboard, CommandQueue);
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