using System.Collections.Immutable;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public interface ICommandOutputViewFactory
{
    Task<View> HandleOutput(string command, string[] output);
    ImmutableArray<string> SupportedCommands { get; }
    bool IsSupported(string command);
}

public interface IOutputParser
{
    public CommandOutput<OutputLine> Parse(string command, string[] output);
}

public abstract record CommandOutputViewFactoryBase<TOutputParser>(IClipboard Clipboard) : ICommandOutputViewFactory
    where TOutputParser : IOutputParser, new()
{
    public abstract ImmutableArray<string> SupportedCommands { get; }

    public virtual bool IsSupported(string command) => SupportedCommands.Any(supportedCommand =>
        command.StartsWith(supportedCommand, StringComparison.OrdinalIgnoreCase));

    public async Task<View> HandleOutput(string command, string[] output)
    {
        if (!IsSupported(command))
            throw new NotSupportedException($"{GetType().FullName} does not support command {command}");

        var commandOutput = new TOutputParser().Parse(command, output);

        return await CreateView(commandOutput);
    }

    protected abstract Task<View> CreateView(CommandOutput<OutputLine> output);
}

public abstract record DefaultViewsOutputViewFactoryBase<TParser>(IClipboard Clipboard, CommandQueue CommandQueue) : CommandOutputViewFactoryBase<TParser>(Clipboard)
    where TParser : IOutputParser, new()
{
    protected override Task<View> CreateView(CommandOutput<OutputLine> output)
    {
        var (window, listView, _) = UI.MakeDefaultCommandViews().SetupLogic(Clipboard, output.Lines);

        listView.KeyPress += args =>
        {
            if (args.KeyEvent.Key == Key.Enter)
            {
                if (listView.GetSelectedOutput<OutputLine>() is { } line)
                {
                    if (SubcommandsView.TryGetSubcommandsDialog(line, Clipboard, CommandQueue) is {} dialog)
                        Application.Run(dialog);

                    args.Handled = true;
                }
            }
        };

        return Task.FromResult((View) window);
    }
}