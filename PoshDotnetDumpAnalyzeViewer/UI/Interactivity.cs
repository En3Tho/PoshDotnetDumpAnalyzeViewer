using PoshDotnetDumpAnalyzeViewer.Views;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.Interactivity;

public static class ViewsExtensions
{
    public static ListView FixNavigationDown(this ListView @this)
    {
        @this.KeyDown += (_, args) =>
        {
            switch (args.KeyCode)
            {
                case KeyCode.End:
                    @this.SelectedItem = @this.Source.Length - 1;
                    @this.TopItem = Math.Max(0, @this.Source.Length - @this.Frame.Height);
                    args.Handled = true;
                    break;
                case KeyCode.PageDown:
                    var jumpSize = @this.Frame.Height;
                    var jumpPoint = @this.SelectedItem;
                    var jumpMax = @this.Source.Length - @this.Frame.Height;
                    if (jumpPoint + jumpSize > jumpMax)
                    {
                        @this.SelectedItem = Math.Min(@this.Source.Length - 1, jumpPoint + jumpSize);
                        @this.TopItem = Math.Max(0, jumpMax);
                        args.Handled = true;
                    }

                    break;
            }
        };
        return @this;
    }

    public static ArrayListView<T> AddClipboard<T>(this ArrayListView<T> @this, IClipboard clipboard)
    {
        @this.KeyDown += (_, args) =>
        {
            switch (args.KeyCode)
            {
                case KeyCode.CtrlMask | KeyCode.C:
                    clipboard.SetClipboardData(@this.Source[@this.SelectedItem]!.ToString());
                    args.Handled = true;
                    break;

                case KeyCode.CtrlMask | KeyCode.ShiftMask | KeyCode.C:
                    clipboard.SetClipboardData(string.Join(Environment.NewLine, @this.Source));
                    args.Handled = true;
                    break;
            }
        };
        return @this;
    }

    public static ArrayListView<T> HandleEnter<T>(this ArrayListView<T> @this,
        Func<T, Toplevel?> dialogFactory,
        Func<Exception, bool> exceptionHandler)
    {
        @this.KeyDown += (_, args) =>
        {
            if (args.KeyCode == KeyCode.Enter)
            {
                if (dialogFactory(@this.Source[@this.SelectedItem]) is { } dialog)
                {
                    Application.Run(dialog, exceptionHandler);
                }
                args.Handled = true;
            }
        };

        return @this;
    }

    public static ArrayListView<T> LinkWithFilterField<T>(this ArrayListView<T> @this, TextField filter, Func<T, string, bool> filterPredicate)
    {
        var filterHistory = new HistoryList<string>();
        var lastFilter = "";

        @this.KeyDown += (_, args) =>
        {
            switch (args.KeyCode)
            {
                case KeyCode.Tab:
                    FindNextListItem();
                    args.Handled = true;
                    break;
                default:
                    // delegate simple number and letter keystrokes to filter
                    // TODO: backspace is not processed anymore in v2. A bug?
                    if (args.KeyCode is >= KeyCode.Space and <= KeyCode.Z or KeyCode.Backspace)
                    {
                        filter.OnProcessKeyDown(args.KeyCode);
                        args.Handled = true;
                    }
                    break;
            }
        };

        void FilterListItems()
        {
            var filterText = filter.Text;
            if (lastFilter.Equals(filterText)) return;

            if (string.IsNullOrEmpty(filterText))
            {
                @this.SetSource(@this.InitialSource);
            }
            else
            {
                var filteredOutput =
                    @this.InitialSource
                        .Where(x => filterPredicate(x, filterText))
                        .ToArray();

                @this.SetSource(filteredOutput);
                filterHistory.Add(filterText);
            }

            lastFilter = filterText ?? "";
        }

        void FindNextListItem()
        {
            var filterText = filter.Text;

            if (string.IsNullOrWhiteSpace(filterText))
                return;

            @this.TryFindItemAndSetSelected(x => filterPredicate(x, filterText));
        }

        filter
            .AddCommandHistory(filterHistory)
            .KeyDown += (_, args) =>
        {
            switch (args.KeyCode)
            {
                case KeyCode.Enter:
                    FilterListItems();
                    args.Handled = true;
                    break;
                case KeyCode.Tab:
                    FindNextListItem();
                    args.Handled = true;
                    break;
            }
        };

        return @this;
    }
}

public static class CommandOutputViewExtensions
{
    public static CommandOutputView AddDefaultBehavior(this CommandOutputView @this, IClipboard clipboard)
    {
        @this.ListView
            .AddClipboard(clipboard)
            .LinkWithFilterField(@this.Filter, (line, filter) => line.Contains(filter, StringComparison.OrdinalIgnoreCase))
            .FixNavigationDown();

        @this.Filter.AddClipboard(clipboard);

        return @this;
    }
}

public static class MainLayoutExtensions
{
    public static MainLayout SetupLogic(this MainLayout @this, TabManager tabManager, CommandQueue commandQueue, IClipboard clipboard,
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