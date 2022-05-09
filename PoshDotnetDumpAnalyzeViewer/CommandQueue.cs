using System.Threading.Channels;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public record CommandQueue(DotnetDumpAnalyzeBridge DotnetDump, HistoryList<string> CommandHistory, TabManager TabManager, CancellationToken Token)
{
    private readonly Channel<string> _channel = Channel.CreateUnbounded<string>(new() { SingleReader = true});
    public void SendCommand(string command)
    {
        _channel.Writer.TryWrite(command);
    }

    void Start() => Task.Run(async () =>
    {
        var reader = _channel.Reader;
        await foreach (var command in reader.ReadAllAsync(Token))
        {
            if (TabManager.TrySetSelectedExistingTab(command))
                continue;

            var handler = (ICommandHandler)null; // TODO: serviceProvider.GetRequiredService(Type);
            var view = await handler.HandleCommand(DotnetDump, command);
            var tab =
                new TabView.Tab(command, view)
                   .AddTabClosing(TabManager);
            CommandHistory.AddCommand(command);
            TabManager.SetTab(command, tab);
        }
    }, Token);
}