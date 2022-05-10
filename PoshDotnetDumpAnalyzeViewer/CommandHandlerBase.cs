using System.Collections.Immutable;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public interface ICommandHandler
{
    Task<View> HandleCommand(DotnetDumpAnalyzeBridge dotnetDump, string command);
    ImmutableArray<string> SupportedCommands { get; }
    bool IsSupported(string command);
}

public interface IOutputParser
{
    public CommandOutput<OutputLine> Parse(string command, string[] output, bool isOk);
}

public abstract record CommandHandlerBase<TOutputParser>(IClipboard Clipboard) : ICommandHandler
    where TOutputParser : IOutputParser, new()
{
    public abstract ImmutableArray<string> SupportedCommands { get; }

    public virtual bool IsSupported(string command) => SupportedCommands.Any(supportedCommand =>
        command.StartsWith(supportedCommand, StringComparison.OrdinalIgnoreCase));

    public async Task<View> HandleCommand(DotnetDumpAnalyzeBridge dotnetDump, string command)
    {
        if (!IsSupported(command))
            throw new NotSupportedException($"{GetType().FullName} does not support command {command}");

        var output = await dotnetDump.PerformCommand<TOutputParser>(command);

        if (!output.IsOk)
            return UI.MakeDefaultCommandViews().SetupLogic(Clipboard, output.Lines).Window;

        return await ProcessOutput(output);
    }

    protected abstract Task<View> ProcessOutput(CommandOutput<OutputLine> output);
}