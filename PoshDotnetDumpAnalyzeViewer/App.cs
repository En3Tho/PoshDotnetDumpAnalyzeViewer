﻿using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public static class App
{
    public static async Task Run(string analyzeArgs)
    {
        var process = await ProcessUtil.StartDotnetDumpAnalyze(analyzeArgs);
        Application.Init();

        var source = new CancellationTokenSource();
        var bridge = new DotnetDumpAnalyzeBridge(process, source.Token);
        var topLevelViews = UI.MakeViews(Application.Top);
        var tabManager = new TabManager(topLevelViews.TabView);
        var clipboard = new MiniClipboard(Application.Driver.Clipboard);
        var historyList = new HistoryList<string>();

        Application.Top.Closing += _ =>
        {
            source.Cancel();
            process.Kill(true);
        };

        var exceptionHandler = UI.MakeExceptionHandler(tabManager, clipboard);
        var commandQueue = new CommandQueue(exn => exceptionHandler(exn));

        var viewFactories = new ICommandOutputViewFactory[]
        {
            new QuitCommandOutputViewFactory(clipboard),
            new HelpCommandOutputViewFactory(clipboard, commandQueue),
            new DumpHeapCommandOutputViewFactory(topLevelViews, clipboard, commandQueue),
            new ObjSizeCommandOutputViewFactory(topLevelViews, clipboard, commandQueue),
            new SetThreadCommandOutputViewFactory(topLevelViews, clipboard, commandQueue),
            new ClrThreadsCommandOutputViewFactory(topLevelViews, clipboard, commandQueue),
            new SyncBlockCommandOutputViewFactory(topLevelViews, clipboard, commandQueue),
            new DumpObjectCommandOutputViewFactory(topLevelViews, clipboard, commandQueue),
            new GCRootCommandOutputViewFactory(topLevelViews, clipboard, commandQueue),
            (SosCommandOutputViewFactory)null!, // this slot is for sos, it's sorta special as it delegates output parsing to other factories
            new DefaultCommandOutputViewFactory(clipboard)
        };

        viewFactories[^2] = new SosCommandOutputViewFactory(topLevelViews, clipboard, commandQueue, viewFactories);

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