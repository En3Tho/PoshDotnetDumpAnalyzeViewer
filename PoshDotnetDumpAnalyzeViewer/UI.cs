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
    public static CommandOutputViews SetupLogic(this CommandOutputViews @this, IClipboard clipboard,
        string[] initialSource)
    {
        @this.OutputListView.SetSource(initialSource);

        @this.OutputListView.KeyPress += args =>
        {
            switch (args.KeyEvent.Key)
            {
                case Key.CtrlMask | Key.C:
                    clipboard.SetClipboardData(@this.OutputListView.Source[@this.OutputListView.SelectedItem]);
                    args.Handled = true;
                    break;

                case Key.CtrlMask | Key.ShiftMask | Key.C:
                    clipboard.SetClipboardData(string.Join(Environment.NewLine, @this.OutputListView.Source));
                    args.Handled = true;
                    break;
                case Key.Tab:
                    ProcessTabKey();
                    args.Handled = true;
                    break;
            }
        };

        var filterHistory = new HistoryList<string>();
        var lastFilter = "";

        void ProcessEnterKey()
        {
            var filter = @this.FilterTextField.Text?.ToString();
            if (lastFilter.Equals(filter)) return;

            if (string.IsNullOrEmpty(filter))
            {
                @this.OutputListView.SetSource(initialSource);
            }
            else
            {
                var filteredOutput =
                    initialSource
                        .Where(line => line.Contains(filter, StringComparison.OrdinalIgnoreCase))
                        .ToArray();

                @this.OutputListView.SetSource(filteredOutput);
                filterHistory.Add(filter);
            }

            lastFilter = filter ?? "";
        }

        void ProcessTabKey()
        {
            var filter = @this.FilterTextField.Text?.ToString();

            if (string.IsNullOrWhiteSpace(filter))
                return;

            @this.OutputListView.TryFindItemAndSetSelected(x => x.Contains(filter, StringComparison.OrdinalIgnoreCase));
        }

        @this.FilterTextField
            .AddClipboard(clipboard)
            .AddCommandHistory(filterHistory)
            .KeyPress += args =>
        {
            switch (args.KeyEvent.Key)
            {
                case Key.Enter:
                    ProcessEnterKey();
                    args.Handled = true;
                    break;
                case Key.Tab:
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
        @this.TabView.KeyPress += args =>
        {
            switch (args.KeyEvent.Key)
            {
                case Key.CtrlMask | Key.W:
                {
                    // special case help
                    if (@this.TabView is { SelectedTab: {} selectedTab} && selectedTab.Text.ToString() is not "help")
                        tabManager.RemoveTab(selectedTab);
                    args.Handled = true;
                    break;
                }
                case Key.CtrlMask | Key.R:
                {
                    // special case help
                    if (@this.TabView is { SelectedTab: {} selectedTab } && selectedTab.Text.ToString()! is not "help" and var tabText)
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
            var command = @this.CommandInput.Text?.ToString();
            if (string.IsNullOrWhiteSpace(command)) return;
            commandQueue.SendCommand(command, forceRefresh: forceRefresh);
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

            switch (args.KeyEvent.Key)
            {
                case Key.CtrlMask | Key.Enter:
                    ProcessEnterKey(true);
                    args.Handled = true;
                    break;
                case Key.Enter:
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

            var commandViews =
                MakeDefaultCommandViews()
                    .SetupLogic(clipboard, errorSource);

            var tab =
                new TabView.Tab("Unhandled exception", commandViews.Window);

            tabManager.AddTab(exn.Message, commandViews, tab, false);
            return true;
        };
    }

    public static CommandOutputViews MakeDefaultCommandViews()
    {
        var window = new Window
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var listView = new ArrayListView<string>(Array.Empty<string>())
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
}