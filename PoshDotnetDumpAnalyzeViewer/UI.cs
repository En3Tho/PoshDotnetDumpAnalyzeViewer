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
    public static DefaultCommandViews SetupLogic<T>(this DefaultCommandViews @this, IClipboard clipboard,
        T[] initialSource)
        where T : IOutputLine
    {
        var lastFilter = "";

        @this.OutputListView.SetSource(initialSource);

        @this.OutputListView.KeyPress += args =>
        {
            switch (args.KeyEvent.Key)
            {
                case Key.CtrlMask | Key.C:
                    clipboard.SetClipboardData(@this.OutputListView.Source.ToList()[@this.OutputListView.SelectedItem]
                        ?.ToString());
                    args.Handled = true;
                    break;

                case Key.CtrlMask | Key.ShiftMask | Key.C:
                    if (@this.OutputListView.GetSource<IList<object>>() is { } source)
                    {
                        clipboard.SetClipboardData(string.Join(Environment.NewLine, source.Select(x => x.ToString())));
                        args.Handled = true;
                    }
                    break;
                case Key.Tab:
                    ProcessTabKey();
                    args.Handled = true;
                    break;
            }
        };

        var filterHistory = new HistoryList<string>();

        void ProcessTabKey()
        {
            var filter = (@this.FilterTextField.Text ?? ustring.Empty).ToString()!;
            if (string.IsNullOrEmpty(filter))
                return;

            if (@this.OutputListView.GetSource<IList<object>>() is { Count: > 0 } source)
            {
                bool SetSelectedItem(int index)
                {
                    while (index < source!.Count)
                    {
                        if (source[index].ToString()!.Contains(filter!, StringComparison.OrdinalIgnoreCase))
                        {
                            if (!@this.OutputListView.HasFocus)
                                @this.OutputListView.SetFocus();
                            // display this item in the middle of the list if there is enough space left
                            var linesInList = @this.OutputListView.Bounds.Height;
                            var topItemIndex =
                                index < linesInList
                                    ? 0
                                    : index - linesInList / 2;

                            @this.OutputListView.TopItem = topItemIndex;
                            @this.OutputListView.SelectedItem = index;
                            return true;
                        }

                        index++;
                    }

                    return false;
                }

                var index = @this.OutputListView.SelectedItem + 1;
                if (!SetSelectedItem(index))
                    SetSelectedItem(0);
            }
        }

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
                filterHistory.Add(filter);
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
                    if (@this.TabView is { SelectedTab: {} selectedTab} && selectedTab.Text.ToString() != "help")
                        tabManager.RemoveTab(selectedTab);
                    args.Handled = true;
                    break;
                }
                case Key.CtrlMask | Key.R:
                {
                    // special case help
                    if (@this.TabView is { SelectedTab: { } selectedTab } && selectedTab.Text.ToString()! is not "help" and var tabText)
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
            var command = (@this.CommandInput.Text ?? ustring.Empty).ToString()!;
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
                    .Split(Environment.NewLine)
                    .Select(x => new OutputLine(x)).ToArray();

            var commandViews =
                MakeDefaultCommandViews()
                    .SetupLogic(clipboard, errorSource);

            var tab =
                new TabView.Tab("Unhandled exception", commandViews.Window);

            tabManager.AddTab(exn.Message, tab, false);
            return true;
        };
    }

    public static DefaultCommandViews MakeDefaultCommandViews()
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