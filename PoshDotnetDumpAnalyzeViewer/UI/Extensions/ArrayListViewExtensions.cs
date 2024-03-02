using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.UI.Extensions;

public static class ArrayListViewExtensions
{
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

    public static OutputLine? TryParseLine<TParser>(this ArrayListView<string> @this, string command)
        where TParser : IOutputParser
    {
        var selectedItem = @this.SelectedItem;
        if (@this.Source is { } source && selectedItem >= 0)
            return TParser.Parse(source[selectedItem], command);

        return null;
    }

    public static void TryFindItemAndSetSelected<T>(this ArrayListView<T> @this, Func<T, bool> filter)
    {
        if (@this.Source is not { Length: > 0 } source)
            return;

        bool SetSelectedItem(int start, int end)
        {
            while (start < end)
            {
                if (filter(source[start]))
                {
                    if (!@this.HasFocus)
                        @this.SetFocus();
                    // display this item in the middle of the list if there is enough space left
                    var linesInList = @this.Bounds.Height;
                    var topItemIndex =
                        start < linesInList - 1
                            ? 0
                            : start - linesInList / 2;

                    @this.TopItem = topItemIndex;
                    @this.SelectedItem = start;
                    return true;
                }

                start++;
            }

            return false;
        }

        var index = @this.SelectedItem + 1;
        if (!SetSelectedItem(index, source.Length))
            SetSelectedItem(0, @this.SelectedItem);
    }
}