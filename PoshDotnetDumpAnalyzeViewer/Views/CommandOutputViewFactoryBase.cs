using System.Collections.Immutable;
using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using PoshDotnetDumpAnalyzeViewer.ViewBehavior;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.Views;

public interface IOutputParser
{
    static abstract OutputLine Parse(string line, string command);
}

public abstract record CommandOutputViewFactoryBase(IClipboard Clipboard)
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

public abstract record ParsedCommandOutputViewFactoryBase<TParser>(MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : CommandOutputViewFactoryBase(Clipboard)
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
                    return SubcommandsDialog.TryCreate(MainLayout, outputLine, Clipboard, CommandQueue);
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