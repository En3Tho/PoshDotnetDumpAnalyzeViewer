using System.Diagnostics;
using NStack;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public record TopLevelViews(
    Toplevel Toplevel,
    Window Window,
    TabView TabView,
    TextField CommandInput);

public record DefaultCommandViews(
    Window Window,
    ListView OutputListView,
    TextField FilterTextField);

public static class CommandViewsExtensions
{
    public static DefaultCommandViews SetupLogic<T>(this DefaultCommandViews @this, IClipboard clipboard, T[] initialSource) where T : IOutputLine<T>
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
                        .Where(line => line.Line.Contains(filter, StringComparison.OrdinalIgnoreCase))
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

    public static TabView.Tab AddTabClosing(this TabView.Tab @this, TabManager tabManager)
    {
        @this.View.KeyPress += args =>
        {
            if (args.KeyEvent.Key != (Key.CtrlMask | Key.W)) return;
            if (!@this.View.HasFocus) return;

            tabManager.RemoveTab(@this);
            args.Handled = true;
        };
        return @this;
    }
}

public static class ViewsExtensions
{
    public static TopLevelViews SetupLogic(this TopLevelViews @this, IClipboard clipboard, TabManager tabManager, DotnetDumpAnalyzeBridge bridge)
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

                        if (tabManager.TryGetTab(command) is { } existingTab)
                        {
                            @this.TabView.SelectedTab = existingTab;
                        }
                        else
                        {
                            var commandResultTab = await UI.SendCommand(bridge, clipboard, command);
                            var tab =
                                new TabView.Tab(command, commandResultTab.Window)
                                    .AddTabClosing(tabManager);
                            commandHistory.AddCommand(command);
                            tabManager.SetTab(command, tab);
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
    public static async Task<DefaultCommandViews> SendCommand(DotnetDumpAnalyzeBridge bridge,
        IClipboard clipboard, string command)
    {
        var result = await bridge.PerformCommand<DefaultOutputLine>(command);
        var commandResultViews = MakeDefaultCommandViews(command).SetupLogic(clipboard, result.Lines);
        return commandResultViews;
    }

    public static Func<Exception, bool> MakeExceptionHandler(TabManager tabManager, IClipboard clipboard)
    {
        return exn =>
        {
            var errorSource =
                exn.ToString()
                   .Split(Environment.NewLine)
                   .Select(x => new DefaultOutputLine(x)).ToArray();

            var commandViews =
                MakeDefaultCommandViews("Unhandled exception")
                    .SetupLogic(clipboard, errorSource);

            var tab =
                new TabView.Tab("Unhandled exception", commandViews.Window)
                    .AddTabClosing(tabManager);

            tabManager.SetTab(exn.Message, tab);
            return true;
        };
    }

    public static DefaultCommandViews MakeDefaultCommandViews(string command)
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

        return new(window, listView, filterField);
    }

    private static TopLevelViews MakeViews(Toplevel toplevel)
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

        var topLevelViews = MakeViews(Application.Top);
        var tabManager = new TabManager(Application.MainLoop, topLevelViews.TabView);

        topLevelViews.SetupLogic(clipboard, tabManager, bridge);
        var exceptionHandler = MakeExceptionHandler(tabManager, clipboard);

        // TODO: better way to await operation and dispatch command
        SpinWaitTask(Task.Run(async () =>
        {
            var helpCommandViews = await SendCommand(bridge, clipboard, "help");
            var tab = new TabView.Tab("Unhandled exception", helpCommandViews.Window);
            tabManager.SetTab("help", tab);
            // var commands = Parser.Help.GetCommandsFromOutput(output.Lines);
            // topLevelViews.CommandInput.Autocomplete.AllSuggestions = new(commands);
        }, source.Token));

        Application.Run(topLevelViews.Toplevel, exceptionHandler);
        Application.Shutdown();
    }
}