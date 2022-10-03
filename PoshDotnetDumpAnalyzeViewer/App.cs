using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public class TerminalAppBuilder<TTopLevel> where TTopLevel : Toplevel
{
    public TerminalAppBuilder()
    {
        Application.Init();
    }

    public IServiceCollection Services { get; } = new ServiceCollection();

    public void Run(Func<IServiceProvider, TTopLevel> topLevelFactory, Func<IServiceProvider, Func<Exception, bool>> handlerFactory)
    {
        Services.AddSingleton(Application.MainLoop);
        var provider = Services.BuildServiceProvider();
        var topLevel = topLevelFactory(provider);
        var exceptionHandler = handlerFactory(provider);
        Application.Run(topLevel, exceptionHandler);
        Application.Shutdown();
    }
}

public static class App
{
    // Not sure this is worth it ?
    public static async Task Run2(string analyzeArgs)
    {
        var builder = new TerminalAppBuilder<Toplevel>();
        var process = await ProcessUtil.StartDotnetDumpAnalyze(analyzeArgs);
        var source = new CancellationTokenSource();
        var bridge = new DotnetDumpAnalyzeBridge(process, source.Token);

        var viewsFactory = UI.MakeViews(Application.Top).SetupLogic;
        builder.Services
            .AddSingletonFunc(viewsFactory)
            .TryAddSingletonOrFail(bridge)
            .TryAddSingletonOrFail<TabManager>()
            .TryAddSingletonOrFail<IClipboard, MiniClipboard>()
            .TryAddScopedOrFail<HistoryList<string>>()
            .TryAddSingletonOrFail<QuitCommandOutputViewFactory>()
            .TryAddSingletonOrFail<HelpCommandOutputViewFactory>()
            .TryAddSingletonOrFail<DumpHeapCommandOutputViewFactory>()
            .TryAddSingletonOrFail<SetThreadCommandOutputViewFactory>()
            .TryAddSingletonOrFail<DefaultCommandOutputViewFactory>();

        builder.Run(
            services =>
            {
                Application.Top.Closing += _ =>
                {
                    source.Cancel();
                    process.Kill(true);
                };

                var commandQueue = services.GetRequiredService<CommandQueue>();
                var worker = services.GetRequiredService<CommandQueueWorker>();
                commandQueue.Start(worker, source.Token);

                Application.Top.Loaded += () => { commandQueue.SendCommand("help"); };

                return services.GetRequiredService<TopLevelViews>().Toplevel;
            },
            services =>
            {
                var handlerFactory = UI.MakeExceptionHandler;
                return services.Run(handlerFactory);
            });
    }

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
            new DumpHeapCommandOutputViewFactory(topLevelViews, clipboard, commandQueue),
            new SetThreadCommandOutputViewFactory(topLevelViews, clipboard, commandQueue),
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