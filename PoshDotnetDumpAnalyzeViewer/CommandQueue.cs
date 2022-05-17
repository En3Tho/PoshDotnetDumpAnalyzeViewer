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
    void UpdateCommandViews(TabView.Tab tabToUpdate, CommandOutputViews views, bool ignoreOutput, Func<CommandOutputViews, CommandOutputViews>? customAction)
    {
        // in case we need something from resulting view
        var updatedView = customAction is { } ? customAction(views) : views;
        if (ignoreOutput)
            return;

        tabToUpdate.View = updatedView.Window;
        TabManager.SetSelected(tabToUpdate);
    }

    // TODO: rewrite this to di based commands maybe ?
    // TODO: too many booleans? -_-
    public async Task Process(string command, bool forceRefresh = false, bool ignoreOutput = false, Func<CommandOutputViews, CommandOutputViews>? customAction = null)
    {
        try
        {
            TopLevelViews.CommandInput.Text = command;
            TopLevelViews.CommandInput.ReadOnly = true;

            if (!forceRefresh && TabManager.TryGetTab(command) is { } tabToUpdate)
            {
                UpdateCommandViews(tabToUpdate.Tab, tabToUpdate.Views, ignoreOutput, customAction);
                return;
            }

            var viewFactory = ViewFactories.First(x => x.IsSupported(command));

            using var cts = new CancellationTokenSource();
            async Task RunTicker(CancellationToken token)
            {

                using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
                var seconds = 0;

                while (!token.IsCancellationRequested)
                {
                    await timer.WaitForNextTickAsync(token);
                    Application.MainLoop.Invoke(() =>
                    {
                        TopLevelViews.CommandInput.Text = $"{command} ... executing command ({seconds++}s)";
                    });
                }
            }

            var _ = RunTicker(cts.Token);
            var result = await cts.AwaitAndCancel(DotnetDump.PerformCommand(command));

            TopLevelViews.CommandInput.Text = $"{command} ... processing output" ;
            var views =
                result.IsOk
                    ? viewFactory.HandleOutput(command, result.Output)
                    : UI.MakeDefaultCommandViews().SetupLogic(Clipboard, result.Output.Map(x => new OutputLine(x)));

            if (ignoreOutput && result.IsOk)
            {
                // in case we need something from resulting view
                customAction?.Invoke(views);
                return;
            }

            CommandHistory.Add(command);

            TabView.Tab tab;
            if (TabManager.TryGetTab(command) is { } tabToUpgrade)
            {
                tabToUpgrade.Tab.View = views.Window;
                TabManager.SetSelected(tabToUpgrade.Tab);
                tab = tabToUpgrade.Tab;
            }
            else
            {
                var newTab = new TabView.Tab(command, views.Window);
                TabManager.AddTab(command, views, newTab,  true);
                tab = newTab;
            }

            // wait for ui to initialize
            if (result.IsOk)
                UpdateCommandViews(tab, views, ignoreOutput, customAction);
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
    private readonly Channel<(string, bool, bool, Func<CommandOutputViews, CommandOutputViews>?)> _channel =
        Channel.CreateUnbounded<(string, bool, bool, Func<CommandOutputViews, CommandOutputViews>?)>(new() { SingleReader = true});

    public void SendCommand(string command,  bool forceRefresh = false, bool ignoreOutput = false, Func<CommandOutputViews, CommandOutputViews>? customAction = null)
    {
        _channel.Writer.TryWrite((command, forceRefresh, ignoreOutput, customAction));
    }

    public void Start(CommandQueueWorker worker, CancellationToken token) => Task.Run(async () =>
    {
        var reader = _channel.Reader;
        await foreach (var (command, forceRefresh, ignoreOutput, customAction) in reader.ReadAllAsync(token))
        {
            try
            {
                await worker.Process(command, forceRefresh, ignoreOutput, customAction);
            }
            catch (Exception exn)
            {
                ExceptionHandler(exn);
            }
        }
    }, token);
}