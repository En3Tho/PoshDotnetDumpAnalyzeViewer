using System.Collections.Immutable;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public interface ICommandHandler
{
    Task<View> HandleCommand(DotnetDumpAnalyzeBridge dotnetDump, string command);
}

public abstract record CommandHandler<T>(IClipboard Clipboard) : ICommandHandler
    where T : IOutputLine<T>
{
    public abstract ImmutableArray<string> SupportedCommands { get; }

    public virtual bool IsSupported(string command) => SupportedCommands.Any(supportedCommand =>
        command.StartsWith(supportedCommand, StringComparison.OrdinalIgnoreCase));

    public async Task<View> HandleCommand(DotnetDumpAnalyzeBridge dotnetDump, string command)
    {
        if (!IsSupported(command))
            throw new NotSupportedException($"{GetType().FullName} does not support command {command}");

        var output = await dotnetDump.PerformCommand<T>(command);

        if (!output.IsOk)
            return UI.MakeDefaultCommandViews(command).SetupLogic(Clipboard, output.Lines).Window;

        return await Run(output);
    }

    protected abstract Task<View> Run(CommandOutput<T> output);

    private async Task<View> Run(string command, CommandOutput<T> output)
    {
        if (!IsSupported(command))
            throw new NotSupportedException($"{GetType().FullName} does not support command {command}");

        if (!output.IsOk)
            return UI.MakeDefaultCommandViews(command).SetupLogic(Clipboard, output.Lines).Window;

        return await Run(output);
    }
}