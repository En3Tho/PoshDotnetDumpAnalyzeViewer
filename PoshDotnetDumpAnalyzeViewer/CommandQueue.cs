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
            return tabToUpgrade.Tab;
        }

        var newTab = new TabView.Tab(command, views.Window);
        TabManager.AddTab(command, views, newTab,  true);
        return newTab;
    }

    // TODO: rewrite this to di based commands maybe ?
    // TODO: too many booleans? -_-
    public async UITask Process(string command, string commandTabName, bool forceRefresh = false, bool ignoreOutput = false, Func<CommandOutputViews, CommandOutputViews>? customAction = null)
    {
        var textToRestore = TopLevelViews.CommandInput.Text?.ToString();
        if (command.Equals(textToRestore)) textToRestore = "";

        try
        {
            TopLevelViews.CommandInput.Text = command;
            TopLevelViews.CommandInput.ReadOnly = true;

            if (!forceRefresh && TabManager.TryGetTab(commandTabName) is { } tabToUpdate)
            {
                var updatedView = customAction?.Invoke(tabToUpdate.Views) ?? tabToUpdate.Views;
                if (ignoreOutput)
                    return;

                UpdateTab(tabToUpdate.Tab, updatedView);
                return;
            }

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
            var result = await cts.AwaitAndCancel(Task.Run(() => DotnetDump.PerformCommand(command)));
            var output = new CommandOutput(command, result.Output);

            if (!result.IsOk)
            {
                var errorViews = UI.MakeDefaultCommandViews(output).SetupLogic(Clipboard, output);
                GetOrCreateTabForCommand(commandTabName, errorViews);
                return;
            }

            var viewFactory = ViewFactories.First(x => x.IsSupported(command));
            var views = viewFactory.HandleOutput(output);
            var updatedViews = customAction?.Invoke(views) ?? views;

            if (ignoreOutput)
            {
                // in case we need something from resulting view
                updatedViews.Window.Dispose();
                return;
            }

            CommandHistory.Add(command);
            var tab = GetOrCreateTabForCommand(commandTabName, updatedViews);
            UpdateTab(tab, updatedViews);
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
    private readonly Channel<(string, string, bool, bool, Func<CommandOutputViews, CommandOutputViews>?)> _channel =
        Channel.CreateUnbounded<(string, string, bool, bool, Func<CommandOutputViews, CommandOutputViews>?)>(new() { SingleReader = true});

    public void SendCommand(string command, string commandTabName,  bool forceRefresh = false, bool ignoreOutput = false, Func<CommandOutputViews, CommandOutputViews>? customAction = null)
    {
        _channel.Writer.TryWrite((command, commandTabName, forceRefresh, ignoreOutput, customAction));
    }

    public void SendCommand(string command, bool forceRefresh = false, bool ignoreOutput = false, Func<CommandOutputViews, CommandOutputViews>? customAction = null)
    {
        _channel.Writer.TryWrite((command, command, forceRefresh, ignoreOutput, customAction));
    }

    public void Start(CommandQueueWorker worker, CancellationToken token) => Task.Run(async () =>
    {
        var reader = _channel.Reader;
        await foreach (var (command, commandTabName, forceRefresh, ignoreOutput, customAction) in reader.ReadAllAsync(token))
        {
            try
            {
                await worker.Process(Commands.NormalizeCommand(command), commandTabName, forceRefresh, ignoreOutput, customAction);
            }
            catch (Exception exn)
            {
                ExceptionHandler(exn);
            }
        }
    }, token);
}