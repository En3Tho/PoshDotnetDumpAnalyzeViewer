using System.Collections.Immutable;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.CommandHandlers;

public abstract record CommandHandler(DotnetDumpAnalyzeBridge DotnetDump, IClipboard Clipboard)
{
    public abstract ImmutableArray<string> SupportedCommands { get; }

    public virtual bool IsSupported(string command) => SupportedCommands.Any(supportedCommand =>
        command.StartsWith(supportedCommand, StringComparison.OrdinalIgnoreCase));

    protected abstract Task<View> Run(CommandOutput output);

    public async Task<View> Run(string command)
    {
        if (!IsSupported(command))
            throw new NotSupportedException($"{GetType().FullName} does not support command {command}");

        var output = await DotnetDump.PerformCommand(command);

        if (!output.IsOk)
            return UI.MakeDefaultCommandViews(command).SetupLogic(Clipboard, output.Lines).Window;

        return await Run(output);
    }
}

public record DefaultCommandHandler(DotnetDumpAnalyzeBridge DotnetDump, IClipboard Clipboard) : CommandHandler(DotnetDump, Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } = new();
    public override bool IsSupported(string command) => true;
    protected override Task<View> Run(CommandOutput output)
    {
        throw new NotImplementedException();
    }
}

public record QuitCommandHandler(DotnetDumpAnalyzeBridge DotnetDump, IClipboard Clipboard) : CommandHandler(DotnetDump, Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } =
        new() { Commands.Exit, Commands.Q, Commands.Quit };

    public override bool IsSupported(string command) =>
        SupportedCommands.Any(supportedCommand => command.Equals(supportedCommand, StringComparison.OrdinalIgnoreCase));

    protected override Task<View> Run(CommandOutput output)
    {
        Environment.Exit(0);
        return Task.FromResult<View>(null);
    }
}

public record HelpCommandHandler(DotnetDumpAnalyzeBridge DotnetDump, IClipboard Clipboard) : CommandHandler(DotnetDump, Clipboard)
{
    public override ImmutableArray<string> SupportedCommands { get; } = new() { Commands.Help };

    public override bool IsSupported(string command) =>
        command.Equals(Commands.Help, StringComparison.OrdinalIgnoreCase);

    protected override Task<View> Run(CommandOutput output)
    {
        var startOfActualCommands = output.Lines.TakeAfter("Commands:");

        //defaultCommandViews.OutputListView.mous

        throw new NotImplementedException();
    }
}