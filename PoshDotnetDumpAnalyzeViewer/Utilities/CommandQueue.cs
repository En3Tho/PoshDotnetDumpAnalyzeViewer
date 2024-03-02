using System.Threading.Channels;
using PoshDotnetDumpAnalyzeViewer.OutputViewFactories;
using PoshDotnetDumpAnalyzeViewer.Tasks;
using PoshDotnetDumpAnalyzeViewer.ViewBehavior;
using PoshDotnetDumpAnalyzeViewer.Views;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.Utilities;

public record CommandQueueWorker(
    IClipboard Clipboard,
    DotnetDump DotnetDump,
    MainLayout MainLayout,
    HistoryList<string> CommandHistory,
    TabManager TabManager,
    IEnumerable<CommandViewFactoryBase> ViewFactories)
{
    private void UpdateTab(Tab tabToUpdate, View view)
    {
        tabToUpdate.View = view;
        TabManager.SetSelected(tabToUpdate);
    }

    Tab GetOrCreateTabForCommand(string command, View view)
    {
        if (TabManager.TryGetTab(command) is { } tabToUpgrade)
        {
            return tabToUpgrade.Tab;
        }

        var newTab = new Tab
        {
            DisplayText = command,
            View = view
        };

        TabManager.AddTab(command, view, newTab,  true);
        return newTab;
    }

    // TODO: rewrite this to di based commands maybe ?
    // TODO: too many booleans? -_-
    public async UITask Process(string command, string commandTabName, bool forceRefresh = false, bool ignoreOutput = false,
        Func<View, View>? mapView = null,
        Func<string[], string[]>? mapOutput = null)
    {
        var textToRestore = MainLayout.CommandInput.Text;
        if (command.Equals(textToRestore)) textToRestore = "";

        try
        {
            var viewFactory = ViewFactories.First(x => x.IsSupported(command));
            command = viewFactory.NormalizeCommand(command);

            MainLayout.CommandInput.Text = command;
            MainLayout.CommandInput.ReadOnly = true;

            if (!forceRefresh && TabManager.TryGetTab(commandTabName) is { } tabToUpdate)
            {
                var updatedView = mapView?.Invoke(tabToUpdate.View) ?? tabToUpdate.View;
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
                    MainLayout.CommandInput.Text = $"{command} ... executing command ({seconds++}s)";
                    await timer.WaitForNextTickAsync(token);
                }
            }

            _ = RunTicker(cts.Token);
            var result = await cts.AwaitAndCancel(Task.Run(() => DotnetDump.Run(command)));

            if (!result.IsOk)
            {
                var errorViews = new CommandOutputView(result.Output).AddDefaultBehavior(Clipboard);
                GetOrCreateTabForCommand(commandTabName, errorViews);
                return;
            }

            var lines = mapOutput?.Invoke(result.Output) ?? result.Output;
            var output = new CommandOutput(command, lines);
            var views = viewFactory.HandleOutput(output);
            var updatedView2 = mapView?.Invoke(views) ?? views;

            if (ignoreOutput)
            {
                // in case we need something from resulting view
                updatedView2.Dispose();
                return;
            }

            CommandHistory.Add(command);
            var tab = GetOrCreateTabForCommand(commandTabName, updatedView2);
            UpdateTab(tab, updatedView2);
        }
        finally
        {
            MainLayout.CommandInput.ReadOnly = false;
            MainLayout.CommandInput.Text = textToRestore;
        }
    }
}

public record CommandQueue(Action<Exception> ExceptionHandler)
{
    private readonly Channel<(string, string, bool, bool, Func<View, View>?, Func<string[], string[]>?)> _channel =
        Channel.CreateUnbounded<(string, string, bool, bool, Func<View, View>?, Func<string[], string[]>?)>(new() { SingleReader = true});

    public void SendCommand(string command, string commandTabName,  bool forceRefresh = false, bool ignoreOutput = false, Func<View, View>? mapView = null, Func<string[], string[]>? mapOutput = null)
    {
        _channel.Writer.TryWrite((command, commandTabName, forceRefresh, ignoreOutput, mapView, mapOutput));
    }

    public void SendCommand(string command, bool forceRefresh = false, bool ignoreOutput = false, Func<View, View>? mapView = null, Func<string[], string[]>? mapOutput = null)
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