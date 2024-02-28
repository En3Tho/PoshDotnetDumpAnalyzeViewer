using System.Threading.Channels;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public record CommandQueueWorker(
    IClipboard Clipboard,
    DotnetDumpAnalyzeBridge DotnetDump,
    TopLevelViews TopLevelViews,
    HistoryList<string> CommandHistory,
    TabManager TabManager,
    IEnumerable<CommandOutputViewFactoryBase> ViewFactories)
{
    private void UpdateTab(Tab tabToUpdate, CommandOutputViews views)
    {
        tabToUpdate.View = views.Window;
        TabManager.SetSelected(tabToUpdate);
    }

    Tab GetOrCreateTabForCommand(string command, CommandOutputViews views)
    {
        if (TabManager.TryGetTab(command) is { } tabToUpgrade)
        {
            return tabToUpgrade.Tab;
        }

        var newTab = new Tab
        {
            DisplayText = command,
            View = views.Window
        };

        TabManager.AddTab(command, views, newTab,  true);
        return newTab;
    }

    // TODO: rewrite this to di based commands maybe ?
    // TODO: too many booleans? -_-
    public async UITask Process(string command, string commandTabName, bool forceRefresh = false, bool ignoreOutput = false,
        Func<CommandOutputViews, CommandOutputViews>? mapView = null,
        Func<string[], string[]>? mapOutput = null)
    {
        var textToRestore = TopLevelViews.CommandInput.Text;
        if (command.Equals(textToRestore)) textToRestore = "";

        try
        {
            var viewFactory = ViewFactories.First(x => x.IsSupported(command));
            command = viewFactory.NormalizeCommand(command);

            TopLevelViews.CommandInput.Text = command;
            TopLevelViews.CommandInput.ReadOnly = true;

            if (!forceRefresh && TabManager.TryGetTab(commandTabName) is { } tabToUpdate)
            {
                var updatedView = mapView?.Invoke(tabToUpdate.Views) ?? tabToUpdate.Views;
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

            if (!result.IsOk)
            {
                var errorOutput = new CommandOutput(command, result.Output);
                var errorViews = UI.MakeDefaultCommandViews(errorOutput).SetupLogic(Clipboard, errorOutput);
                GetOrCreateTabForCommand(commandTabName, errorViews);
                return;
            }

            var lines = mapOutput?.Invoke(result.Output) ?? result.Output;
            var output = new CommandOutput(command, lines);
            var views = viewFactory.HandleOutput(output);
            var updatedViews = mapView?.Invoke(views) ?? views;

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
    private readonly Channel<(string, string, bool, bool, Func<CommandOutputViews, CommandOutputViews>?, Func<string[], string[]>?)> _channel =
        Channel.CreateUnbounded<(string, string, bool, bool, Func<CommandOutputViews, CommandOutputViews>?, Func<string[], string[]>?)>(new() { SingleReader = true});

    public void SendCommand(string command, string commandTabName,  bool forceRefresh = false, bool ignoreOutput = false, Func<CommandOutputViews, CommandOutputViews>? mapView = null, Func<string[], string[]>? mapOutput = null)
    {
        _channel.Writer.TryWrite((command, commandTabName, forceRefresh, ignoreOutput, mapView, mapOutput));
    }

    public void SendCommand(string command, bool forceRefresh = false, bool ignoreOutput = false, Func<CommandOutputViews, CommandOutputViews>? mapView = null, Func<string[], string[]>? mapOutput = null)
    {
        _channel.Writer.TryWrite((command, command, forceRefresh, ignoreOutput, mapView, mapOutput));
    }

    public void Start(CommandQueueWorker worker, CancellationToken token) => Task.Run(async () =>
    {
        var reader = _channel.Reader;
        await foreach (var (command, commandTabName, forceRefresh, ignoreOutput, mapView, mapOutput) in reader.ReadAllAsync(token))
        {
            try
            {
                await worker.Process(Commands.NormalizeCommand(command), commandTabName, forceRefresh, ignoreOutput, mapView, mapOutput);
            }
            catch (Exception exn)
            {
                ExceptionHandler(exn);
            }
        }
    }, token);
}