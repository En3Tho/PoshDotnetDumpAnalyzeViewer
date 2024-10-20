using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.UI.Behavior;

// v2 notes: remove KeyCode
// args to (_, args)
// args.Key to args.KeyCode

using KeyCode = Key;

public static class ListViewExtensions
{

    public static void SetSelectedClipTop(this ListView @this, int selectedItem, bool tryDisplayInTheMiddle = false)
    {
        @this.SelectedItem = selectedItem;
        if (selectedItem + @this.Frame.Height > @this.Source.Count)
        {
            @this.TopItem = Math.Max(0, @this.Source.Count - @this.Frame.Height);
        } else if (tryDisplayInTheMiddle)
        {
            @this.TopItem = Math.Max(0, @this.SelectedItem - @this.Frame.Height / 2);
        }
    }

    public static ListView ClipNavigationDown(this ListView @this)
    {
        @this.KeyDown += args =>
        {
            switch (args.KeyEvent.Key)
            {
                case KeyCode.End:
                    @this.SetSelectedClipTop(@this.Source.Count - 1);
                    args.Handled = true;
                    break;
                case KeyCode.PageDown:
                    @this.SetSelectedClipTop(Math.Min(@this.Source.Count - 1, @this.SelectedItem + @this.Frame.Height));
                    args.Handled = true;
                    break;
            }
        };
        return @this;
    }
}

public static class ArrayListViewExtensions
{
    public static ArrayListView<T> AddClipboard<T>(this ArrayListView<T> @this, IClipboard clipboard)
    {
        @this.KeyUp += args =>
        {
            switch (args.KeyEvent.Key)
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
        @this.KeyDown += args =>
        {
            if (args.KeyEvent.Key == KeyCode.Enter)
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

        @this.KeyDown += args =>
        {
            var key = args.KeyEvent.Key;
            switch (key)
            {
                case KeyCode.Tab:
                    FindNextListItem();
                    args.Handled = true;
                    break;
                default:
                    // delegate simple number and letter keystrokes to filter
                    // TODO: backspace is not processed anymore in v2. A bug?
                    if (key is >= KeyCode.Space and <= KeyCode.z or KeyCode.Backspace)
                    {
                        filter.ProcessKey(args.KeyEvent);
                        args.Handled = true;
                    }
                    break;
            }
        };

        void FilterListItems(T[] source)
        {
            var filterText = filter.Text?.ToString();
            if (lastFilter.Equals(filterText)) return;

            if (string.IsNullOrEmpty(filterText))
            {
                @this.SetSource(source);
            }
            else
            {
                var filteredOutput =
                    source
                        .Where(x => filterPredicate(x, filterText))
                        .ToArray();

                @this.SetSource(filteredOutput);
                filterHistory.Add(filterText);
            }

            lastFilter = filterText ?? "";
        }

        void FindNextListItem()
        {
            var filterText = filter.Text?.ToString();

            if (string.IsNullOrWhiteSpace(filterText))
                return;

            @this.TryFindItemAndSetSelected(x => filterPredicate(x, filterText));
        }

        filter
            .AddCommandHistory(filterHistory)
            .KeyDown += args =>
        {
            switch (args.KeyEvent.Key)
            {
                case KeyCode.Enter:
                    FilterListItems(@this.Source);
                    args.Handled = true;
                    break;
                case KeyCode.CtrlMask | KeyCode.Enter:
                    FilterListItems(@this.InitialSource);
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

                    @this.SetSelectedClipTop(start, true);
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