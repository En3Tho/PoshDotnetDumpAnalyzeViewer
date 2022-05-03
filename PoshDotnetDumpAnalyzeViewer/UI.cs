using System.Diagnostics;
using NStack;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

// in theory in memory bus with commands of different types is better? try it? maybe on redis viewer?

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
    public static CommandViews SetupLogic(this CommandViews @this, MiniClipboard clipboard, string command, string[] initialSource)
    {
        var lastFilter = "";

        @this.Tab.Text = command;
        @this.OutputListView.SetSource(initialSource);

        @this.OutputListView.KeyPress += args =>
        {
            switch (args.KeyEvent.Key)
            {
                case Key.CtrlMask | Key.C:
                    clipboard.Set(@this.OutputListView.Source.ToList()[@this.OutputListView.SelectedItem]?.ToString());
                    args.Handled = true;
                    break;
            }
        };

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
                var filteredOutput = initialSource.Where(outputString => outputString.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToArray();
                @this.OutputListView.SetSource(filteredOutput);
            }

            lastFilter = filter;
        }


        @this.FilterTextField.KeyPress += args =>
        {
            switch (args.KeyEvent.Key)
            {
                case Key.Enter:
                    ProcessEnterKey();
                    args.Handled = true;
                    break;

                case Key.CtrlMask | Key.C:
                    @this.FilterTextField.Copy(clipboard);
                    args.Handled = true;
                    break;

                case Key.CtrlMask | Key.V:
                    if (clipboard.Get() is { } text)
                        @this.FilterTextField.Paste(text);
                    args.Handled = true;
                    break;
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
    public static Views SetupLogic(this Views @this, MiniClipboard clipboard, TabManager tabManager, DotnetDumpAnalyzeBridge bridge)
    {
        var semaphore = new SemaphoreSlim(1);
        var errorHandler = UI.MakeExceptionHandler(tabManager, clipboard);
        var commandHistory = new CommandHistory();

        void ProcessEnterKey()
        {
            @this.CommandInput.Enabled = false;
            var work =
                Task.Run(async () =>
                {
                    try
                    {
                        var command = (@this.CommandInput.Text ?? ustring.Empty).ToString()!;
                        if (string.IsNullOrEmpty(command)) return;

                        if (tabManager.TryGetTab(command) is { Output.IsOk: true } tabInfo)
                        {
                            @this.TabView.SelectedTab = tabInfo.Views.Tab;
                        }
                        else
                        {
                            var commandResultTab = await UI.SendCommand(bridge, clipboard, tabManager, command);
                            commandResultTab.Item1.AddTabClosing(tabManager);
                            tabManager.SetTab(command, commandResultTab);
                            commandHistory.AddCommand(command);
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

        void ProcessUp()
        {
            if (commandHistory.PreviousCommand() is { } command)
                @this.CommandInput.Text = command;
        }

        void ProcessDown()
        {
            if (commandHistory.NextCommand() is { } command)
                @this.CommandInput.Text = command;
        }

        @this.CommandInput.KeyPress += args =>
        {
            // not sure why but enter key press on filter text filed triggers this one too. A bug?
            if (!@this.CommandInput.HasFocus) return;

            switch (args.KeyEvent.Key)
            {
                case Key.Enter:
                    ProcessEnterKey();
                    args.Handled = true;
                    break;

                case Key.CursorUp:
                    ProcessUp();
                    args.Handled = true;
                    break;

                case Key.CursorDown:
                    ProcessDown();
                    args.Handled = true;
                    break;

                case Key.CtrlMask | Key.C:
                    @this.CommandInput.Copy(clipboard);
                    args.Handled = true;
                    break;

                case Key.CtrlMask | Key.V:
                    if (clipboard.Get() is { } text)
                        @this.CommandInput.Paste(text);
                    args.Handled = true;
                    break;
            }
        };

        return @this;
    }
}

public class UI
{
    public static async Task<(CommandViews, CommandOutput)> SendCommand(DotnetDumpAnalyzeBridge bridge, MiniClipboard clipboard, TabManager tabManager, string command)
    {
        var result = await bridge.PerformCommand(command);
        var commandResultViews = MakeCommandViews().SetupLogic(clipboard, command, result.Output);
        return (commandResultViews, result);
    }

    public static Func<Exception, bool> MakeExceptionHandler(TabManager tabManager, MiniClipboard clipboard)
    {
        return exn =>
        {
            var errorSource = exn.ToString().Split(Environment.NewLine);
            var cmdViews = MakeCommandViews().SetupLogic(clipboard, "Unhandled exception", errorSource).AddTabClosing(tabManager);
            var cmdOutput = new CommandOutput(true, errorSource);
            tabManager.SetTab(exn.Message, (cmdViews, cmdOutput));
            return true;
        };
    }

    public static CommandViews MakeCommandViews()
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

        filterFrame.Add(filterField);

        window.Add(listView);
        window.Add(filterFrame);

        var tab = new TabView.Tab("", window);

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
            Height = Dim.Fill() - Dim.Sized(3)
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

        commandFrame.Add(commandInput);

        window.Add(tabView);
        window.Add(commandFrame);

        toplevel.Add(window);

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

        var views = MakeViews(Application.Top);

        var bridge = new DotnetDumpAnalyzeBridge(dotnetDumpProcess, source.Token);
        var tabManager = new TabManager(views.TabView);
        var clipboard = new MiniClipboard(Application.Driver.Clipboard);

        views.SetupLogic(clipboard, tabManager, bridge);
        var exceptionHandler = MakeExceptionHandler(tabManager, clipboard);

        SpinWaitTask(Task.Run(async () =>
        {
            var (helpCommandTab, output) = await SendCommand(bridge, clipboard, tabManager, "help");
            tabManager.SetTab("help", (helpCommandTab, output));
        }, source.Token));

        Application.Run(views.Toplevel, exceptionHandler);
        Application.Shutdown();
    }
}