using System.Threading.Channels;
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
    private void UpdateTab(TabView.Tab tabToUpdate, CommandOutputViews views)
    {
        var oldViews = tabToUpdate.View;
        if (!ReferenceEquals(oldViews, views.Window))
        {
            // this one removes tab interface for some reason in 1.5
            // and in 1.14 windows just hangs
            // dunno if this is needed at all
            //oldViews.Dispose();
        }
        tabToUpdate.View = views.Window;
        TabManager.SetSelected(tabToUpdate);
    }

    void UpdateCommandViews(TabView.Tab tabToUpdate, CommandOutputViews views, bool ignoreOutput, Func<CommandOutputViews, CommandOutputViews>? customAction)
    {
        // in case we need something from resulting view
        var updatedView = customAction is { } ? customAction(views) : views;
        if (ignoreOutput)
            return;

        UpdateTab(tabToUpdate, updatedView);
    }

    TabView.Tab GetOrCreateTabForCommand(string command, CommandOutputViews views)
    {
        if (TabManager.TryGetTab(command) is { } tabToUpgrade)
        {
            UpdateTab(tabToUpgrade.Tab, views);
            return tabToUpgrade.Tab;
        }

        var newTab = new TabView.Tab(command, views.Window);
        TabManager.AddTab(command, views, newTab,  true);
        return newTab;
    }

    // TODO: rewrite this to di based commands maybe ?
    // TODO: too many booleans? -_-
    public async UITask Process(string command, bool forceRefresh = false, bool ignoreOutput = false, Func<CommandOutputViews, CommandOutputViews>? customAction = null)
    {
        var textToRestore = TopLevelViews.CommandInput.Text?.ToString();
        if (command.Equals(textToRestore)) textToRestore = "";

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
            async UITask RunTicker(CancellationToken token)
            {
                using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
                var seconds = 0;

                while (!token.IsCancellationRequested)
                {
                    TopLevelViews.CommandInput.Text = $"{command} ... executing command ({seconds++}s)";
                    await timer.WaitForNextTickAsync(token);
                }
            }

            _ = RunTicker(cts.Token);
            var result = await cts.AwaitAndCancel(DotnetDump.PerformCommand(command));

            TopLevelViews.CommandInput.Text = $"{command} ... processing output";
            var output = new CommandOutput(command, result.Output);
            var views =
                result.IsOk
                    ? viewFactory.HandleOutput(output)
                    : UI.MakeDefaultCommandViews(output).SetupLogic(Clipboard, output);

            if (ignoreOutput && result.IsOk)
            {
                // in case we need something from resulting view
                customAction?.Invoke(views);
                views.Window.Dispose();
                return;
            }

            CommandHistory.Add(command);
            var tab = GetOrCreateTabForCommand(command, views);
            if (result.IsOk)
                UpdateCommandViews(tab, views, ignoreOutput, customAction);
        }
        finally
        {
            TopLevelViews.CommandInput.ReadOnly = false;
            TopLevelViews.CommandInput.Text = textToRestore;
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
                await worker.Process(Commands.NormalizeCommand(command), forceRefresh, ignoreOutput, customAction);
            }
            catch (Exception exn)
            {
                ExceptionHandler(exn);
            }
        }
    }, token);
}