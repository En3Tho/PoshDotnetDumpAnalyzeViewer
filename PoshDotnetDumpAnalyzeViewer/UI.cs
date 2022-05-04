using System.Diagnostics;
using NStack;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public record Views(
    Toplevel Toplevel,
    Window Window,
    TabView TabView,
    TextField CommandInput);

public record CommandViews(
    TabView.Tab Tab,
    Window Window,
    ListView OutputListView,
    TextField FilterTextField);

public static class CommandViewsExtensions
{
    public static CommandViews SetupLogic(this CommandViews @this, IClipboard clipboard, string[] initialSource)
    {
        var lastFilter = "";

        @this.OutputListView.SetSource(initialSource);

        @this.OutputListView.KeyPress += args =>
        {
            switch (args.KeyEvent.Key)
            {
                case Key.CtrlMask | Key.C:
                    clipboard.SetClipboardData(@this.OutputListView.Source.ToList()[@this.OutputListView.SelectedItem]?.ToString());
                    args.Handled = true;
                    break;
            }
        };

        var commandHistory = new HistoryList<string>();

        void ProcessEnterKey()
        {
            var filter = (@this.FilterTextField.Text ?? ustring.Empty).ToString()!;

            if (lastFilter.Equals(filter)) return;

            if (string.IsNullOrEmpty(filter))
            {
                @this.OutputListView.SetSource(initialSource);
            }
            else
            {
                var filteredOutput =
                    initialSource
                        .Where(outputString => outputString.Contains(filter, StringComparison.OrdinalIgnoreCase))
                        .ToArray();
                @this.OutputListView.SetSource(filteredOutput);
            }

            lastFilter = filter;
            if (!string.IsNullOrWhiteSpace(filter))
                commandHistory.AddCommand(filter);
        }

        @this.FilterTextField
            .AddClipboard(clipboard)
            .AddCommandHistory(commandHistory)
            .KeyPress += args =>
        {
            if (args.KeyEvent.Key == Key.Enter)
            {
                ProcessEnterKey();
                args.Handled = true;
            }
        };

        return @this;
    }

    public static CommandViews AddTabClosing(this CommandViews @this, TabManager tabManager)
    {
        @this.Tab.View.KeyPress += args =>
        {
            if (args.KeyEvent.Key != (Key.CtrlMask | Key.W)) return;
            if (!@this.Tab.View.HasFocus) return;

            tabManager.RemoveTab(@this.Tab);
            args.Handled = true;
        };
        return @this;
    }
}

public static class ViewsExtensions
{
    public static Views SetupLogic(this Views @this, IClipboard clipboard, TabManager tabManager, DotnetDumpAnalyzeBridge bridge)
    {
        var semaphore = new SemaphoreSlim(1);
        var errorHandler = UI.MakeExceptionHandler(tabManager, clipboard);
        var commandHistory = new HistoryList<string>();

        void ProcessEnterKey()
        {
            @this.CommandInput.Enabled = false;
            var work =
                Task.Run(async () =>
                {
                    try
                    {
                        var command = (@this.CommandInput.Text ?? ustring.Empty).ToString()!;
                        if (string.IsNullOrWhiteSpace(command)) return;

                        if (tabManager.TryGetTab(command) is { Output.IsOk: true } tabInfo)
                        {
                            @this.TabView.SelectedTab = tabInfo.Views.Tab;
                        }
                        else
                        {
                            var commandResultTab = await UI.SendCommand(bridge, clipboard, command);
                            commandResultTab.Views.AddTabClosing(tabManager);
                            commandHistory.AddCommand(command);

                            tabManager.SetTab(command, commandResultTab);
                        }

                        @this.CommandInput.Text = ustring.Empty;
                    }
                    finally
                    {
                        @this.CommandInput.Enabled = true;
                    }
                });

            var _ =
                semaphore.RunTask(work.WithErrorHandler(exn => errorHandler(exn)));
        }

        @this.CommandInput
            .AddClipboard(clipboard)
            .AddCommandHistory(commandHistory)
            .KeyPress += args =>
        {
            // TODO: check if this is a bug and try to fix it
            // TODO: try to fix double finger scroll in gui.cs?
            // not sure why but enter key press on filter text filed triggers this one too. A bug?
            if (!@this.CommandInput.HasFocus) return;

            if (args.KeyEvent.Key == Key.Enter)
            {
                ProcessEnterKey();
                args.Handled = true;
            }
        };

        return @this;
    }
}

public class UI
{
    public static async Task<(CommandViews Views, CommandOutput Output)> SendCommand(DotnetDumpAnalyzeBridge bridge,
        IClipboard clipboard, string command)
    {
        var result = await bridge.PerformCommand(command);
        var commandResultViews = MakeCommandViews(command).SetupLogic(clipboard, result.Output);
        return (commandResultViews, result);
    }

    public static Func<Exception, bool> MakeExceptionHandler(TabManager tabManager, IClipboard clipboard)
    {
        return exn =>
        {
            var errorSource = exn.ToString().Split(Environment.NewLine);
            var cmdViews =
                MakeCommandViews("Unhandled exception")
                    .SetupLogic(clipboard, errorSource)
                    .AddTabClosing(tabManager);
            var cmdOutput = new CommandOutput(true, errorSource);
            tabManager.SetTab(exn.Message, (cmdViews, cmdOutput));
            return true;
        };
    }

    public static CommandViews MakeCommandViews(string command)
    {
        var window = new Window
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var listView = new ListView
        {
            Height = Dim.Fill() - Dim.Sized(3),
            Width = Dim.Fill()
        };

        var filterFrame = new FrameView("Filter")
        {
            Y = Pos.Bottom(listView),
            Height = 3,
            Width = Dim.Fill()
        };

        var filterField = new TextField
        {
            Height = 1,
            Width = Dim.Fill()
        };

        window.With(
            listView,
            filterFrame.With(
                filterField));

        var tab = new TabView.Tab(command, window);

        return new(tab, window, listView, filterField);
    }

    private static Views MakeViews(Toplevel toplevel)
    {
        var window = new Window
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var tabView = new TabView
        {
            Width = Dim.Fill(),
            Height = Dim.Fill() - Dim.Sized(3),
        };

        var commandFrame = new FrameView("Command")
        {
            Y = Pos.Bottom(tabView),
            Height = 3,
            Width = Dim.Fill()
        };

        var commandInput = new TextField
        {
            Width = Dim.Fill(),
            Height = 1
        };

        toplevel.With(
            window.With(
                tabView,
                commandFrame.With(
                    commandInput)));

        return new(toplevel, window, tabView, commandInput);
    }

    public static void SpinWaitTask(Task task)
    {
        while (!task.IsCompleted) { }

        task.GetAwaiter().GetResult();
    }

    public static void Run(Process dotnetDumpProcess)
    {
        Application.Init();

        var source = new CancellationTokenSource();
        Application.Top.Closing += _ => source.Cancel();

        var bridge = new DotnetDumpAnalyzeBridge(dotnetDumpProcess, source.Token);
        var clipboard = new MiniClipboard(Application.Driver.Clipboard);

        var views = MakeViews(Application.Top);
        var tabManager = new TabManager(Application.MainLoop, views.TabView);

        views.SetupLogic(clipboard, tabManager, bridge);
        var exceptionHandler = MakeExceptionHandler(tabManager, clipboard);

        // TODO: better way to await operation and dispatch command
        SpinWaitTask(Task.Run(async () =>
        {
            var (helpCommandTab, output) = await SendCommand(bridge, clipboard, "help");
            tabManager.SetTab("help", (helpCommandTab, output));
        }, source.Token));

        Application.Run(views.Toplevel, exceptionHandler);
        Application.Shutdown();
    }
}