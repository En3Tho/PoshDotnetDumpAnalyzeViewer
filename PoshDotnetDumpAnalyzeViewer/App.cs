using PoshDotnetDumpAnalyzeViewer.OutputViewFactories;
using PoshDotnetDumpAnalyzeViewer.Tasks;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using PoshDotnetDumpAnalyzeViewer.ViewBehavior;
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

        var exceptionHandler = ViewExceptionHandler.Create(tabManager, clipboard);
        var commandQueue = new CommandQueue(exn => exceptionHandler(exn));

        CommandViewFactoryBase[] viewFactories =
        [
            new QuitCommandViewFactory(clipboard),
            new HelpCommandViewFactory(clipboard, commandQueue, mainLayout),
            new DumpHeapCommandViewFactory(mainLayout, clipboard, commandQueue),
            new ObjSizeCommandViewFactory(mainLayout, clipboard, commandQueue),
            new SetThreadCommandViewFactory(mainLayout, clipboard, commandQueue),
            new ClrThreadsCommandViewFactory(mainLayout, clipboard, commandQueue),
            new SyncBlockCommandViewFactory(mainLayout, clipboard, commandQueue),
            new DumpObjectCommandViewFactory(mainLayout, clipboard, commandQueue),
            new DumpAssemblyCommandViewFactory(mainLayout, clipboard, commandQueue),
            new DumpClassCommandViewFactory(mainLayout, clipboard, commandQueue),
            new DumpMethodTableCommandViewFactory(mainLayout, clipboard, commandQueue),
            new DumpDomainCommandViewFactory(mainLayout, clipboard, commandQueue),
            new DumpModuleCommandViewFactory(mainLayout, clipboard, commandQueue),
            new Name2EeCommandViewFactory(mainLayout, clipboard, commandQueue),
            new GcRootCommandViewFactory(mainLayout, clipboard, commandQueue),
            new PrintExceptionFactory(mainLayout, clipboard, commandQueue),
            new DumpExceptionFactory(mainLayout, clipboard, commandQueue),
            new ParallelStacksViewFactory(mainLayout, clipboard, commandQueue),
            new ClrStackViewFactory(mainLayout, clipboard, commandQueue),
            (SosCommandViewFactory)null!, // this slot is for sos, it's sorta special as it delegates output parsing to other factories
            new DefaultCommandViewFactory(clipboard)
        ];

        viewFactories[^2] = new SosCommandViewFactory(mainLayout, clipboard, commandQueue, viewFactories);

        var commandQueueWorker = new CommandQueueWorker(clipboard, bridge, mainLayout, historyList, tabManager, viewFactories);

        mainLayout.AddDefaultBehavior(tabManager, commandQueue, clipboard, historyList);

        commandQueue.Start(commandQueueWorker, source.Token);

        Application.Top.Loaded += (_, _) =>
        {
            commandQueue.SendCommand("help");
        };

        Application.Run(Application.Top.With(mainLayout), exceptionHandler);
        Application.Shutdown();
    }
}