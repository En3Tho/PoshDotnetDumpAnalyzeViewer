using System.Collections.Immutable;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public interface ICommandOutputViewFactory
{
    CommandOutputViews HandleOutput(string command, string[] output);
    ImmutableArray<string> SupportedCommands { get; }
    bool IsSupported(string command);
}

public interface IOutputParser
{
    static abstract OutputLine Parse(string line, string command);
}

public abstract record CommandOutputViewFactoryBase(IClipboard Clipboard) : ICommandOutputViewFactory
{
    public abstract ImmutableArray<string> SupportedCommands { get; }

    public virtual bool IsSupported(string command) => SupportedCommands.Any(supportedCommand =>
        command.StartsWith(supportedCommand, StringComparison.OrdinalIgnoreCase));

    public CommandOutputViews HandleOutput(string command, string[] output)
    {
        if (!IsSupported(command))
            throw new NotSupportedException($"{GetType().FullName} does not support command {command}");

        var commandOutput = new CommandOutput(command, output);

        return CreateView(commandOutput);
    }

    protected abstract CommandOutputViews CreateView(CommandOutput output);
}

public abstract record DefaultViewsOutputViewFactoryBase<TParser>(TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue) : CommandOutputViewFactoryBase(Clipboard)
    where TParser : IOutputParser, new()
{
    protected override CommandOutputViews CreateView(CommandOutput output)
    {
        var views = UI.MakeDefaultCommandViews().SetupLogic(Clipboard, output.Lines);

        views.OutputListView.KeyPress += args =>
        {
            if (args.KeyEvent.Key == Key.Enter)
            {
                if (views.OutputListView.TryParseLine<TParser>(output.Command) is { } line)
                {
                    if (SubcommandsView.TryGetSubcommandsDialog(TopLevelViews, line, Clipboard, CommandQueue) is { } dialog)
                    {
                        Application.Run(dialog, ex =>
                        {
                            CommandQueue.ExceptionHandler(ex);
                            return true;
                        });
                    }


                    args.Handled = true;
                }
            }
        };

        return views;
    }
}