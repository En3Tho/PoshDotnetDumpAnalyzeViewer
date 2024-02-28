using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public class ArrayListView<T> : ListView
{
    public ArrayListView(T[] initialSource)
    {
        SetSource(initialSource);
    }

    public void SetSource(T[] source) => base.SetSource(source);

    public new T[] Source => (T[]) base.Source.ToList();
}

public record TopLevelViews(
    Toplevel Toplevel,
    Window Window,
    TabView TabView,
    TextField CommandInput);

public record CommandOutputViews(
    Window Window,
    ArrayListView<string> OutputListView,
    TextField FilterTextField);

public static class CommandViewsExtensions
{
    public static CommandOutputViews SetupLogic(this CommandOutputViews @this, IClipboard clipboard, CommandOutput output)
    {
        var filterHistory = new HistoryList<string>();
        var lastFilter = "";

        var outputListView = @this.OutputListView;
        outputListView.SetSource(output.Lines);

        outputListView.KeyDown += (_, args) =>
        {
            switch (args.KeyCode)
            {
                case KeyCode.CtrlMask | KeyCode.C:
                    clipboard.SetClipboardData(outputListView.Source[outputListView.SelectedItem]);
                    args.Handled = true;
                    break;

                case KeyCode.CtrlMask | KeyCode.ShiftMask | KeyCode.C:
                    clipboard.SetClipboardData(string.Join(Environment.NewLine, outputListView.Source));
                    args.Handled = true;
                    break;
                case KeyCode.Tab:
                    ProcessTabKey();
                    args.Handled = true;
                    break;
                case KeyCode.CtrlMask | KeyCode.Enter:
                    ProcessEnterKey();
                    args.Handled = true;
                    break;
                case KeyCode.End:
                    outputListView.SelectedItem = outputListView.Source.Length - 1;
                    outputListView.TopItem = Math.Max(0, outputListView.Source.Length - outputListView.Frame.Height);
                    args.Handled = true;
                    break;
                case KeyCode.PageDown:
                    var jumpSize = outputListView.Frame.Height;
                    var jumpPoint = outputListView.SelectedItem;
                    var jumpMax = outputListView.Source.Length - outputListView.Frame.Height;
                    if (jumpPoint + jumpSize > jumpMax)
                    {
                        outputListView.SelectedItem = Math.Min(outputListView.Source.Length - 1, jumpPoint + jumpSize);
                        outputListView.TopItem = Math.Max(0, jumpMax);
                        args.Handled = true;
                    }
                    break;
                default:
                    // delegate simple number and letter keystrokes to filter
                    // TODO: backspace is not processed anymore in v2. A bug?
                    if (args.KeyCode is >= KeyCode.Space and <= KeyCode.Z or KeyCode.Backspace)
                    {
                        @this.FilterTextField.OnProcessKeyDown(args.KeyCode);
                        args.Handled = true;
                    }
                    break;
            }
        };

        void ProcessEnterKey()
        {
            var filter = @this.FilterTextField.Text;
            if (lastFilter.Equals(filter)) return;

            if (string.IsNullOrEmpty(filter))
            {
                outputListView.SetSource(output.Lines);
            }
            else
            {
                var filteredOutput =
                    output.Lines
                        .Where(line => line.Contains(filter, StringComparison.OrdinalIgnoreCase))
                        .ToArray();

                outputListView.SetSource(filteredOutput);
                filterHistory.Add(filter);
            }

            lastFilter = filter ?? "";
        }

        void ProcessTabKey()
        {
            var filter = @this.FilterTextField.Text;

            if (string.IsNullOrWhiteSpace(filter))
                return;

            outputListView.TryFindItemAndSetSelected(x => x.Contains(filter, StringComparison.OrdinalIgnoreCase));
        }

        @this.FilterTextField
            .AddClipboard(clipboard)
            .AddCommandHistory(filterHistory)
            .KeyDown += (_, args) =>
        {
            switch (args.KeyCode)
            {
                case KeyCode.Enter:
                    ProcessEnterKey();
                    args.Handled = true;
                    break;
                case KeyCode.Tab:
                    ProcessTabKey();
                    args.Handled = true;
                    break;
            }
        };

        return @this;
    }
}

public static class ViewsExtensions
{
    public static TopLevelViews SetupLogic(this TopLevelViews @this, TabManager tabManager, CommandQueue commandQueue, IClipboard clipboard,
        HistoryList<string> commandHistory)
    {
        @this.TabView.KeyDown += (_, args) =>
        {
            switch (args.KeyCode)
            {
                case KeyCode.CtrlMask | KeyCode.W:
                {
                    // special case help
                    if (@this.TabView is { SelectedTab: { Text: not "help" } selectedTab})
                        tabManager.RemoveTab(selectedTab);
                    args.Handled = true;
                    break;
                }
                case KeyCode.CtrlMask | KeyCode.R:
                {
                    // special case help
                    if (@this.TabView is { SelectedTab.Text: not "help" and var tabText})
                    {
                        if (tabManager.TryGetTab(tabText) is { IsOk: true })
                        {
                            commandQueue.SendCommand(tabText, forceRefresh: true);
                        }
                    }
                    args.Handled = true;
                    break;
                }
            }
        };

        void ProcessEnterKey(bool forceRefresh)
        {
            var command = @this.CommandInput.Text;
            if (string.IsNullOrWhiteSpace(command)) return;
            commandQueue.SendCommand(command, forceRefresh: forceRefresh);
        }

        @this.CommandInput
            .AddClipboard(clipboard)
            .AddCommandHistory(commandHistory)
            .KeyDown += (_, args) =>
        {
            switch (args.KeyCode)
            {
                case KeyCode.CtrlMask | KeyCode.Enter:
                    ProcessEnterKey(true);
                    args.Handled = true;
                    break;
                case KeyCode.Enter:
                    ProcessEnterKey(false);
                    args.Handled = true;
                    break;
            }
        };

        return @this;
    }
}

public class UI
{
    public static Func<Exception, bool> MakeExceptionHandler(TabManager tabManager, IClipboard clipboard)
    {
        return exn =>
        {
            var errorSource =
                exn.ToString()
                    .Split(Environment.NewLine);

            var commandOutput = new CommandOutput(exn.Message, errorSource);
            var commandViews =
                MakeDefaultCommandViews(commandOutput)
                    .SetupLogic(clipboard, commandOutput);

            var tab = new Tab
            {
                Title = "Unhandled exception",
                View = commandViews.Window
            };
            tabManager.AddTab(exn.Message, commandViews, tab, false);

            return true;
        };
    }

    public static CommandOutputViews MakeDefaultCommandViews(CommandOutput output)
    {
        var window = new Window
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var listView = new ArrayListView<string>(Array.Empty<string>())
        {
            Height = Dim.Fill() - Dim.Sized(2),
            Width = Dim.Fill(),
        };

        var filterFrame = new FrameView
        {
            Title = "Filter",
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

    public static TopLevelViews MakeViews(Toplevel toplevel)
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

        var commandFrame = new FrameView
        {
            Title = "Command",
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
}