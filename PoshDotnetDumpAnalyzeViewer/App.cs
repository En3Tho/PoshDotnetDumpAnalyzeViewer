using PoshDotnetDumpAnalyzeViewer.Interactivity;
using PoshDotnetDumpAnalyzeViewer.Views;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public static class App
{
    public const string DotnetDumpToolName = "dotnet-dump";

    public static async Task Run(string fileName, string analyzeArgs)
    {
        var process = await ProcessUtil.StartDotnetDumpAnalyze(fileName, analyzeArgs);
        Application.Init();
        UISynchronizationContext.Set(SynchronizationContext.Current ?? throw new InvalidOperationException("SynchronizationContext.Current is null"));

        var source = new CancellationTokenSource();
        var bridge = new DotnetDump(process, source.Token);
        var mainLayout = new MainLayout();
        var tabManager = new TabManager(mainLayout.TabView);
        var clipboard = new MiniClipboard(Application.Driver.Clipboard);
        var historyList = new HistoryList<string>();

        Application.Top.Closing += (_, _) =>
        {
            source.Cancel();
            process.Kill(true);
        };

        var exceptionHandler = UI.MakeExceptionHandler(tabManager, clipboard);
        var commandQueue = new CommandQueue(exn => exceptionHandler(exn));

        CommandOutputViewFactoryBase[] viewFactories =
        [
            new QuitCommandOutputViewFactory(clipboard),
            new HelpCommandOutputViewFactory(clipboard, commandQueue, mainLayout),
            new DumpHeapCommandOutputViewFactory(mainLayout, clipboard, commandQueue),
            new ObjSizeCommandOutputViewFactory(mainLayout, clipboard, commandQueue),
            new SetThreadCommandOutputViewFactory(mainLayout, clipboard, commandQueue),
            new ClrThreadsCommandOutputViewFactory(mainLayout, clipboard, commandQueue),
            new SyncBlockCommandOutputViewFactory(mainLayout, clipboard, commandQueue),
            new DumpObjectCommandOutputViewFactory(mainLayout, clipboard, commandQueue),
            new DumpAssemblyCommandOutputViewFactory(mainLayout, clipboard, commandQueue),
            new DumpClassCommandOutputViewFactory(mainLayout, clipboard, commandQueue),
            new DumpMethodTableCommandOutputViewFactory(mainLayout, clipboard, commandQueue),
            new DumpDomainCommandOutputViewFactory(mainLayout, clipboard, commandQueue),
            new DumpModuleCommandOutputViewFactory(mainLayout, clipboard, commandQueue),
            new Name2EECommandOutputViewFactory(mainLayout, clipboard, commandQueue),
            new GCRootCommandOutputViewFactory(mainLayout, clipboard, commandQueue),
            new PrintExceptionOutputFactory(mainLayout, clipboard, commandQueue),
            new DumpExceptionOutputFactory(mainLayout, clipboard, commandQueue),
            new ParallelStacksOutputFactory(mainLayout, clipboard, commandQueue),
            new ClrStackOutputViewFactory(mainLayout, clipboard, commandQueue),
            (SosCommandOutputViewFactory)null!, // this slot is for sos, it's sorta special as it delegates output parsing to other factories
            new DefaultCommandOutputViewFactory(clipboard)
        ];

        viewFactories[^2] = new SosCommandOutputViewFactory(mainLayout, clipboard, commandQueue, viewFactories);

        var commandQueueWorker = new CommandQueueWorker(clipboard, bridge, mainLayout, historyList, tabManager, viewFactories);

        mainLayout.SetupLogic(tabManager, commandQueue, clipboard, historyList);

        commandQueue.Start(commandQueueWorker, source.Token);

        Application.Top.Loaded += (_, _) =>
        {
            commandQueue.SendCommand("help");
        };

        Application.Run(Application.Top.With(mainLayout), exceptionHandler);
        Application.Shutdown();
    }
}