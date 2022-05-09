using System.Threading.Channels;
using NStack;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public record CommandQueueWorker(
    DotnetDumpAnalyzeBridge DotnetDump,
    TopLevelViews TopLevelViews,
    HistoryList<string> CommandHistory,
    TabManager TabManager,
    IEnumerable<ICommandHandler> Handlers)
{
    public async Task Process(string command)
    {
        if (TabManager.TrySetSelectedExistingTab(command))
            return;
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
            TopLevelViews.CommandInput.Text = ustring.Empty;
        }
    }
}

public record CommandQueue()
{
    private readonly Channel<string> _channel = Channel.CreateUnbounded<string>(new() { SingleReader = true});
    public void SendCommand(string command)
    {
        _channel.Writer.TryWrite(command);
    }

    public void Start(CommandQueueWorker worker, CancellationToken token) => Task.Run(async () =>
    {
        var reader = _channel.Reader;
        await foreach (var command in reader.ReadAllAsync(token))
        {
            await worker.Process(command);
        }
    }, token);
}