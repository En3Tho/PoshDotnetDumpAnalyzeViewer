using System.Threading.Channels;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public record CommandQueue(
    DotnetDumpAnalyzeBridge DotnetDump,
    TopLevelViews TopLevelViews,
    HistoryList<string> CommandHistory,
    TabManager TabManager,
    IEnumerable<ICommandHandler> Handlers,
    CancellationToken Token)
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
            try
            {
                TopLevelViews.CommandInput.Text = command;
                TopLevelViews.CommandInput.ReadOnly = true;

                var handler = Handlers.First(x => x.IsSupported(command));
                var view = await handler.HandleCommand(DotnetDump, command);
                var tab =
                    new TabView.Tab(command, view)
                        .AddTabClosing(TabManager);
                CommandHistory.AddCommand(command);
                TabManager.SetTab(command, tab);
            }
            finally
            {
                TopLevelViews.CommandInput.ReadOnly = false;
            }
        }
    }, Token);
}