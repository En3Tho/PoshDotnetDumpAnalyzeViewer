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
    public CommandOutput Parse(string command, string[] output);
}

public abstract record CommandOutputViewFactoryBase<TOutputParser>(IClipboard Clipboard) : ICommandOutputViewFactory
    where TOutputParser : IOutputParser, new()
{
    public abstract ImmutableArray<string> SupportedCommands { get; }

    public virtual bool IsSupported(string command) => SupportedCommands.Any(supportedCommand =>
        command.StartsWith(supportedCommand, StringComparison.OrdinalIgnoreCase));

    public CommandOutputViews HandleOutput(string command, string[] output)
    {
        if (!IsSupported(command))
            throw new NotSupportedException($"{GetType().FullName} does not support command {command}");

        var commandOutput = new TOutputParser().Parse(command, output);

        return CreateView(commandOutput);
    }

    protected abstract CommandOutputViews CreateView(CommandOutput output);
}

public abstract record DefaultViewsOutputViewFactoryBase<TParser>(IClipboard Clipboard, CommandQueue CommandQueue) : CommandOutputViewFactoryBase<TParser>(Clipboard)
    where TParser : IOutputParser, new()
{
    protected override CommandOutputViews CreateView(CommandOutput output)
    {
        var views = UI.MakeDefaultCommandViews().SetupLogic(Clipboard, output.Lines);

        views.OutputListView.KeyPress += args =>
        {
            if (args.KeyEvent.Key == Key.Enter)
            {
                if (views.OutputListView.GetSelectedOutput<OutputLine>() is { } line)
                {
                    if (SubcommandsView.TryGetSubcommandsDialog(line, Clipboard, CommandQueue) is { } dialog)
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