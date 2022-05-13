using System.Threading.Channels;
using NStack;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public record CommandQueueWorker(
    IClipboard Clipboard,
    DotnetDumpAnalyzeBridge DotnetDump,
    TopLevelViews TopLevelViews,
    HistoryList<string> CommandHistory,
    TabManager TabManager,
    IEnumerable<ICommandOutputViewFactory> ViewFactories)
{
    // TODO: rewrite this to di based commands ?
    public async Task Process(string command, bool forceRefresh = false, bool ignoreOutput = false)
    {
        try
        {
            TopLevelViews.CommandInput.Text = command;
            TopLevelViews.CommandInput.ReadOnly = true;

            if (!forceRefresh && TabManager.TrySetSelectedExistingTab(command))
                return;

            var viewFactory = ViewFactories.First(x => x.IsSupported(command));


            var result = await DotnetDump.PerformCommand(command);

            if (ignoreOutput && result.IsOk)
                return;

            var view =
                result.IsOk
                    ? await viewFactory.HandleOutput(command, result.Output)
                    : UI.MakeDefaultCommandViews().SetupLogic(Clipboard, result.Output.Map(x => new OutputLine(x))).Window;

            CommandHistory.Add(command);

            if (TabManager.TryGetTab(command) is { } existingTab)
            {
                existingTab.Tab.View = view;
                TabManager.SetSelected(existingTab.Tab);
            }
            else
            {
                var newTab = new TabView.Tab(command, view);
                TabManager.AddTab(command, newTab, true);
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
    private readonly Channel<(string, bool, bool)> _channel = Channel.CreateUnbounded<(string, bool, bool)>(new() { SingleReader = true});
    public void SendCommand(string command,  bool forceRefresh = false, bool ignoreOutput = false)
    {
        _channel.Writer.TryWrite((command, forceRefresh, ignoreOutput));
    }

    public void Start(CommandQueueWorker worker, CancellationToken token) => Task.Run(async () =>
    {
        var reader = _channel.Reader;
        await foreach (var (command, forceRefresh, ignoreOutput) in reader.ReadAllAsync(token))
        {
            try
            {
                await worker.Process(command, forceRefresh, ignoreOutput);
            }
            catch (Exception exn)
            {
                ExceptionHandler(exn);
            }
        }
    }, token);
}