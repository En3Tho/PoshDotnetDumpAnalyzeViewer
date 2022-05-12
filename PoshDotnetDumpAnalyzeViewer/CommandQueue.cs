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
    // TODO: rewrite this to di based commands ?
    public async Task Process(string command, bool forceCommand = false, bool ignoreOutput = false)
    {
        if (!forceCommand && TabManager.TrySetSelectedExistingTab(command))
            return;
        try
        {
            TopLevelViews.CommandInput.Text = command;
            TopLevelViews.CommandInput.ReadOnly = true;

            var handler = Handlers.First(x => x.IsSupported(command));
            var view = await handler.HandleCommand(DotnetDump, command);

            if (ignoreOutput)
                return;

            CommandHistory.AddCommand(command);

            if (TabManager.TryGetTab(command) is { } existingTab)
            {
                existingTab.View = view;
            }
            else
            {
                var newTab = new TabView.Tab(command, view);
                TabManager.AddTab(command, newTab);
            }
        }
        finally
        {
            TopLevelViews.CommandInput.ReadOnly = false;
            TopLevelViews.CommandInput.Text = ustring.Empty;
        }
    }
}

public record CommandQueue(Action<Exception> ExceptionHandler)
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
            try
            {
                await worker.Process(command);
            }
            catch (Exception exn)
            {
                ExceptionHandler(exn);
            }
        }
    }, token);
}