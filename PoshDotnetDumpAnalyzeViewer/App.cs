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

        topLevelViews.TabView.KeyPress += args =>
        {
            if (args.KeyEvent.Key == (Key.CtrlMask | Key.W))
            {
                // special case help
                if (topLevelViews.TabView is { SelectedTab: {} selectedTab} && selectedTab.Text.ToString() != "help")
                    tabManager.RemoveTab(selectedTab);
            }
        };

        var commandQueue = new CommandQueue();

        var handlers = new ICommandHandler[]
        {
            new QuitCommandHandler(clipboard),
            new HelpCommandHandler(clipboard, commandQueue),
            new DefaultCommandHandler(clipboard)
        };

        var commandQueueWorker = new CommandQueueWorker(bridge, topLevelViews, historyList, tabManager, handlers);

        topLevelViews.SetupLogic(commandQueue, clipboard, historyList);
        var exceptionHandler = UI.MakeExceptionHandler(tabManager, clipboard);

        commandQueue.Start(commandQueueWorker, source.Token);
        commandQueue.SendCommand("help");

        Application.Run(topLevelViews.Toplevel, exceptionHandler);
        Application.Shutdown();
    }
}