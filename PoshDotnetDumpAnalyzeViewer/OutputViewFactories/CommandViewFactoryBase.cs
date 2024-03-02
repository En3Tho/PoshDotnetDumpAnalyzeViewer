using System.Collections.Immutable;
using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Subcommands;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using PoshDotnetDumpAnalyzeViewer.ViewBehavior;
using PoshDotnetDumpAnalyzeViewer.Views;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.OutputViewFactories;

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

public abstract record ParsedCommandViewFactoryBase<TParser>(MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : CommandViewFactoryBase(Clipboard)
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

public sealed record DefaultCommandViewFactory(IClipboard Clipboard) : CommandViewFactoryBase(Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } = [];
    public override bool IsSupported(string command) => true;

    protected override View CreateView(CommandOutput output)
    {
        var view = new CommandOutputView(output.Lines).AddDefaultBehavior(Clipboard);
        return view;
    }
}