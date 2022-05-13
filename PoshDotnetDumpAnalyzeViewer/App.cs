using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public static class App
{
    public static async Task Run(string analyzeArgs)
    {
        var process = await ProcessUtil.StartDotnetDumpAnalyze(analyzeArgs);

        Application.Init();

        var source = new CancellationTokenSource();
        Application.Top.Closing += _ =>
        {
            source.Cancel();
            process.Kill(true);
        };

        var bridge = new DotnetDumpAnalyzeBridge(process, source.Token);
        var topLevelViews = UI.MakeViews(Application.Top);
        var tabManager = new TabManager(Application.MainLoop, topLevelViews.TabView);
        var clipboard = new MiniClipboard(Application.Driver.Clipboard);
        var historyList = new HistoryList<string>();

        var exceptionHandler = UI.MakeExceptionHandler(tabManager, clipboard);
        var commandQueue = new CommandQueue(exn => exceptionHandler(exn));

        var viewFactories = new ICommandOutputViewFactory[]
        {
            new QuitCommandOutputViewFactory(clipboard),
            new HelpCommandOutputViewFactory(clipboard, commandQueue),
            new DumpHeapCommandOutputViewFactory(clipboard, commandQueue),
            new SetThreadCommandOutputViewFactory(clipboard, commandQueue),
            new DefaultCommandOutputViewFactory(clipboard)
        };

        var commandQueueWorker = new CommandQueueWorker(clipboard, bridge, topLevelViews, historyList, tabManager, viewFactories);

        topLevelViews.SetupLogic(tabManager, commandQueue, clipboard, historyList);

        commandQueue.Start(commandQueueWorker, source.Token);

        Application.Top.Loaded += () =>
        {
            commandQueue.SendCommand("help");
        };

        Application.Run(topLevelViews.Toplevel, exceptionHandler);
        Application.Shutdown();
    }
}