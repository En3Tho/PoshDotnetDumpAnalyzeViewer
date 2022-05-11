using System.Collections.Immutable;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public struct DefaultOutputParser : IOutputParser
{
    public CommandOutput<OutputLine> Parse(string command, string[] output, bool isOk)
    {
        return new(command, isOk, output.Map(x => new OutputLine(x)));
    }
}

public sealed record DefaultCommandHandler(IClipboard Clipboard) : CommandHandlerBase<DefaultOutputParser>(Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } = new();
    public override bool IsSupported(string command) => true;

    protected override Task<View> ProcessOutput(CommandOutput<OutputLine> output)
    {
        var views = UI.MakeDefaultCommandViews().SetupLogic(Clipboard, output.Lines);
        return Task.FromResult((View) views.Window);
    }
}

public sealed record QuitCommandHandler(IClipboard Clipboard) : CommandHandlerBase<DefaultOutputParser>(Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } =
        ImmutableArray.Create(Commands.Exit, Commands.Q, Commands.Quit);

    public override bool IsSupported(string command) =>
        SupportedCommands.Any(supportedCommand => command.Equals(supportedCommand, StringComparison.OrdinalIgnoreCase));

    protected override Task<View> ProcessOutput(CommandOutput<OutputLine> output)
    {
        Environment.Exit(0);
        return Task.FromResult<View>(null);
    }
}

public struct HelpOutputParser : IOutputParser
{
    public CommandOutput<OutputLine> Parse(string command, string[] output, bool isOk)
    {
        var commandStartIndex = output.IndexAfter("Commands:");
        var helpCommandsRange = commandStartIndex..;

        return new(command, isOk, output.MapRange(
            x => new(x),
            new RangeMapper<string, OutputLine>(helpCommandsRange, x => new HelpOutputLine(x))));
    }
}

public sealed record HelpCommandHandler
    (IClipboard Clipboard, CommandQueue CommandQueue) : CommandHandlerBase<HelpOutputParser>(Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.Help);

    public override bool IsSupported(string command) =>
        command.Equals(Commands.Help, StringComparison.OrdinalIgnoreCase);

    protected override Task<View> ProcessOutput(CommandOutput<OutputLine> output)
    {
        var (window, listView, _) = UI.MakeDefaultCommandViews().SetupLogic(Clipboard, output.Lines);

        listView.KeyPress += args =>
        {
            // To function ?
            if (args.KeyEvent.Key == Key.Enter)
            {
                if (listView.GetSelectedOutput<IHelpCommand>() is { } line)
                {
                    var command = line.Commands[0];
                    CommandQueue.SendCommand($"help {command}");
                    args.Handled = true;
                }
            }
        };

        return Task.FromResult((View) window);
    }
}

public struct DumpHeapOutputParser : IOutputParser
{
    public CommandOutput<OutputLine> Parse(string command, string[] output, bool isOk)
    {
        var mainIndexesStart =
            output.IndexAfter(x => x.Contains("Address ") && x.Contains(" MT ") && x.Contains(" Size"));

        var statisticsStart = output.IndexAfter("Statistics:");
        // skip statistics header if statistics are present
        if (statisticsStart > 0) statisticsStart++;

        var statisticsEnd = output.IndexBefore(x => x.Contains("Total ") && x.Contains(" objects"));

        var foundSectionEnd = output.IndexBefore(x => x.Contains("Found ") && x.Contains(" objects"));

        // there are 4 lines between main section data and statistics data
        var mainIndexesEnd =
            statisticsStart == -1
                ? foundSectionEnd
                : statisticsStart - 4;

        var (mainRange, mainIndexes) =
            mainIndexesStart == -1
                ? default
                : (new Range(mainIndexesStart, mainIndexesEnd + 1),
                    Parser.DumpHeap.GetDumpHeapHeaderIndices(output[mainIndexesStart - 1]));


        var (statisticsRange, statisticsIndexes) =
            statisticsStart == -1
                ? default
                : (new Range(statisticsStart, statisticsEnd + 1),
                    Parser.DumpHeap.GetDumpHeapStatisticsHeaderIndexes(output[statisticsStart - 1]));

        return new(command, isOk, output.MapRange(
            x => new(x),
            new RangeMapper<string, OutputLine>(mainRange, x => new DumpHeapOutputLine(x, mainIndexes)),
            new RangeMapper<string, OutputLine>(statisticsRange, x => new DumpHeapStatisticsOutputLine(x, statisticsIndexes))
        ));
    }
}

public sealed record DumpHeapCommandHandler
    (IClipboard Clipboard, CommandQueue CommandQueue) : CommandHandlerBase<DumpHeapOutputParser>(Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpHeap);

    protected override Task<View> ProcessOutput(CommandOutput<OutputLine> output)
    {
        var (window, listView, _) = UI.MakeDefaultCommandViews().SetupLogic(Clipboard, output.Lines);

        listView.KeyPress += args =>
        {
            if (args.KeyEvent.Key == Key.Enter)
            {
                if (listView.GetSelectedOutput<OutputLine>() is { } line)
                {
                    if (SubcommandsView.TryGetSubcommandsDialog(line, Clipboard, CommandQueue, out var dialog))
                        Application.Run(dialog);

                    args.Handled = true;
                }
            }
        };

        return Task.FromResult((View) window);
    }
}