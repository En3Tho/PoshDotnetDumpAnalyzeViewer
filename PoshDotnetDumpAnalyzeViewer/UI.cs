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
    public static CommandViews SetupLogic(this CommandViews @this, string command, string[] initialSource)
    {
        var commandText = $"Command: {command}";
        var lastFilter = "";

        @this.Tab.Text = command;
        @this.OutputListView.SetSource(initialSource);

        @this.FilterTextField.KeyPress += args =>
        {
            if (args.KeyEvent.Key != Key.Enter) return;

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
        };

        return @this;
    }
}

public static class TaskExtensions
{
    public static async Task WithErrorHandler(this Task @this, Action<Exception> errorHandler)
    {
        try
        {
            await @this;
        }
        catch (Exception exn)
        {
            errorHandler(exn);
        }
    }
}

public static class SemaphoreSlimExtensions
{
    public static async Task RunTask(this SemaphoreSlim @this, Task task)
    {
        await @this.WaitAsync();
        try
        {
            await task;
        }
        finally
        {
            @this.Release();
        }
    }
}

public static class ViewsExtensions
{
    public static async Task<CommandViews> FireCommand(this Views @this, DotnetDumpAnalyzeBridge bridge, string command)
    {
        var result = await bridge.PerformCommand(command);
        var commandResultViews = UI.MakeCommandViews().SetupLogic(command, result.Output);
        return commandResultViews;
    }

    public static Views SetupLogic(this Views @this, DotnetDumpAnalyzeBridge bridge)
    {
        var tabMap = new Dictionary<string, CommandViews>(StringComparer.OrdinalIgnoreCase);
        var semaphore = new SemaphoreSlim(1);

        var currentCommand = "";

        var errorHandler = UI.MakeExceptionHandler(@this);

        Task.Run(async () =>
        {
            var helpCommandTab = await @this.FireCommand(bridge, "help");
            tabMap.Add("help", helpCommandTab);
            @this.TabView.AddTab(helpCommandTab.Tab, true);
        });

        @this.CommandInput.KeyPress += args =>
        {
            // not sure why but enter key press on filter text filed triggers this one too. A bug?
            if (!@this.CommandInput.HasFocus) return;
            if (args.KeyEvent.Key != Key.Enter) return;

            args.Handled = true;

            @this.CommandInput.Enabled = false;
            var work =
                Task.Run(async () =>
                {
                    try
                    {
                        var command = (@this.CommandInput.Text ?? ustring.Empty).ToString()!;
                        if (currentCommand == command) return;

                        if (tabMap.TryGetValue(command, out var commandViews))
                            @this.TabView.SelectedTab = commandViews.Tab;
                        else
                        {
                            var commandResultTab = await @this.FireCommand(bridge, command);
                            tabMap.Add(command, commandResultTab);
                            @this.TabView.AddTab(commandResultTab.Tab, true);
                        }

                        currentCommand = command;
                        @this.CommandInput.Text = ustring.Empty;
                    }
                    finally
                    {
                        @this.CommandInput.Enabled = true;
                    }
                });

            var _ =
                semaphore.RunTask(work.WithErrorHandler(exn => errorHandler(exn)));
        };

        return @this;
    }
}

public class UI
{
    public static Func<Exception, bool> MakeExceptionHandler(Views @this)
    {
        return exn =>
        {
            var errorSource = exn.StackTrace?.Split(Environment.NewLine) ?? Array.Empty<string>();
            var cmdViews = MakeCommandViews().SetupLogic("Unhandled exception", errorSource);
            @this.TabView.AddTab(cmdViews.Tab, true);
            return true;
        };
    }

    public static string MakeCommandViewsLabelText(string command, string? filter)
    {
        var commandText = $"Command: {command}";
        var filterText = string.IsNullOrEmpty(filter) ? "" : $"Filter: {filter}";
        return string.Join(". ", commandText, filterText);
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
            Height = Dim.Fill() - Dim.Sized(1),
            Width = Dim.Fill()
        };

        var filterField = new TextField
        {
            Y = Pos.Bottom(listView),
            Height = 1,
            Width = Dim.Fill()
        };

        window.Add(listView);
        window.Add(filterField);

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
            Height = Dim.Fill() - Dim.Sized(1)
        };

        var commandInput = new TextField
        {
            Y = Pos.Bottom(tabView),
            Width = Dim.Fill(),
            Height = 1
        };

        window.Add(tabView);
        window.Add(commandInput);

        toplevel.Add(window);

        return new(toplevel, window, tabView, commandInput);
    }

    public static void Run(Process dotnetDumpProcess)
    {
        Application.Init();

        var source = new CancellationTokenSource();
        Application.Top.Closing += _ => source.Cancel();

        var bridge = new DotnetDumpAnalyzeBridge(dotnetDumpProcess, source.Token);

        var views = MakeViews(Application.Top).SetupLogic(bridge);
        var exceptionHandler = MakeExceptionHandler(views);
        Application.Run(views.Toplevel, exceptionHandler);
        Application.Shutdown();
    }
}