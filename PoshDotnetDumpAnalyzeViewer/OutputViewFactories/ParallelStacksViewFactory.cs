using System.Collections.Immutable;
using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Subcommands;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using PoshDotnetDumpAnalyzeViewer.ViewBehavior;
using PoshDotnetDumpAnalyzeViewer.Views;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.OutputViewFactories;

public sealed record ParallelStacksViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : CommandViewFactoryBase(Clipboard)
{
    // special case pstacks for this viewer
    protected override View CreateView(CommandOutput output)
    {
        var lines = output.Lines;
        lines.AsSpan(1).Reverse();

        var view = new ParallelStacksView(output.Lines).AddDefaultBehavior(Clipboard);

        view.ListView.HandleEnter(
            line =>
            {
                Func<SubcommandButtonFactory, IEnumerable<SubcommandButton>> customFactory = factory =>
                {
                    var buttons = new List<SubcommandButton>();

                    if (view.Shrinked)
                    {
                        buttons.Add(factory.MakeButton(SubcommandsPriority.ParallelStacks, "Restore call stacks",
                            () => view.Restore()));
                    }
                    else
                    {
                        buttons.Add(factory.MakeButton(SubcommandsPriority.ParallelStacks, "Shrink call stacks",
                            () => view.Shrink()));
                    }

                    if (view.ListView.TryParseLine<ParallelStacksParser>(output.Command) is ParallelStacksOutputLine line)
                    {
                        foreach (var osThreadId in line.OsThreadIds)
                        {
                            var idAsInt = OsThreadIdReader.Read(osThreadId);

                            foreach (var button in factory.GetSetAsCurrentThreadButtons(osThreadId, idAsInt))
                                buttons.Add(button);
                        }
                    }

                    return buttons;
                };

                return SubcommandsDialog.TryCreate(MainLayout, new(line), customFactory, Clipboard, CommandQueue);
            },
            ex =>
            {
                CommandQueue.ExceptionHandler(ex);
                return true;
            });

        return view;
    }

    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.ParallelStacks);
}