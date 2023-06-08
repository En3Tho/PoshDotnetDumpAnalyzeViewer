using System.Text.RegularExpressions;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public static class TextFieldExtensions
{
    public static TextField AddClipboard(this TextField @this, IClipboard clipboard,
        Key copyKey = Key.CtrlMask | Key.C, Key pasteKey = Key.CtrlMask | Key.V)
    {
        @this.KeyPress += args =>
        {
            if (args.KeyEvent.Key == copyKey)
            {
                @this.Copy(clipboard);
                args.Handled = true;
            }
            else if (args.KeyEvent.Key == pasteKey)
            {
                if (clipboard.GetClipboardData() is { } clipboardData)
                    @this.Paste(clipboardData);
                args.Handled = true;
            }
        };

        return @this;
    }

    public static TextField AddCommandHistory(this TextField @this, HistoryList<string> historyList,
        Key previousCommandKey = Key.CursorUp, Key nextCommandKey = Key.CursorDown)
    {
        @this.KeyPress += args =>
        {
            if (args.KeyEvent.Key == previousCommandKey)
            {
                if (historyList.Previous() is { } previousCommand)
                    @this.Text = previousCommand;
                args.Handled = true;
            }
            else if (args.KeyEvent.Key == nextCommandKey)
            {
                @this.Text = historyList.Next() ?? "";
                args.Handled = true;
            }
        };

        return @this;
    }

    public static void Copy(this TextField @this, IClipboard clipboard)
    {
        clipboard.SetClipboardData(@this.SelectedText is { Length: > 0 }
            ? @this.SelectedText.ToString()
            : @this.Text?.ToString());
    }

    public static void Paste(this TextField @this, string text)
    {
        if (@this.Text is null or { Length: 0 })
        {
            @this.Text = text;
            @this.CursorPosition = @this.Text.Length;
        }
        else if (@this.SelectedText is { Length: > 0 })
        {
            var start = @this.SelectedStart;
            @this.Text = @this.Text.Replace(@this.SelectedText, text, 1);
            @this.ClearAllSelection();
            @this.CursorPosition = start + text.Length;
        }
        else
        {
            var originalPosition = @this.CursorPosition;
            var originalText = @this.Text.ToString()!;
            @this.Text = originalText.Insert(originalPosition, text);
            @this.CursorPosition = originalPosition + text.Length;
        }
    }
}

public static class ArrayListViewExtensions
{
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

        bool SetSelectedItem(int index)
        {
            while (index < source.Length)
            {
                if (filter(source[index]))
                {
                    if (!@this.HasFocus)
                        @this.SetFocus();
                    // display this item in the middle of the list if there is enough space left
                    var linesInList = @this.Bounds.Height;
                    var topItemIndex =
                        index < linesInList - 1
                            ? 0
                            : index - linesInList / 2;

                    @this.TopItem = topItemIndex;
                    @this.SelectedItem = index;
                    return true;
                }

                index++;
            }

            return false;
        }

        var index = @this.SelectedItem + 1;
        if (!SetSelectedItem(index))
            SetSelectedItem(0);
    }
}

public static class CancellationTokenSourceExtensions
{
    public static async Task<T> AwaitAndCancel<T>(this CancellationTokenSource @this, Task<T> job)
    {
        try
        {
            return await job;
        }
        finally
        {
            @this.Cancel();
        }
    }
}

public static class GroupExtensions
{
    public static Range GetRange(this Group @this)
    {
        return
            @this.ValueSpan.Length == 0
                ? new Range()
                : new(@this.Index, @this.Length + @this.Index);
    }
}

public static class MatchExtensions
{
    public static void CopyGroupsRangesTo(this Match @this, Span<Range> ranges)
    {
        var i = 0;
        foreach (var group in @this.Groups.Cast<Group>().Skip(1))
        {
            var range = group.GetRange();
            ranges[i++] = range;
        }
    }
}

public static class ViewExtensions
{
    public static T With<T>(this T @this, View view, params View[] views) where T : View
    {
        @this.Add(view);
        foreach (var nextView in views)
        {
            @this.Add(nextView);
        }

        return @this;
    }
}

public static class SpanExtensions
{
    public static void Map<TIn, TOut>(this Span<TIn> @this, Span<TOut> result, Func<TIn, TOut> mapper)
    {
        if (@this.Length != result.Length)
            throw new ArgumentException("Span's lengths are different");

        for (var i = 0; i < @this.Length; i++)
            result[i] = mapper(@this[i]);
    }
}

public static class ArrayExtensions
{
    public static TOut[] Map<TIn, TOut>(this TIn[] @this, Func<TIn, TOut> mapper)
    {
        var result = new TOut[@this.Length];
        @this.AsSpan().Map(result.AsSpan(), mapper);
        return result;
    }
}

public static class OutputParserExtensions
{
    public static OutputLine[] ParseAll<T>(string[] lines, string command) where T : IOutputParser
    {
        var parser = (string line) => T.Parse(line, command);
        return lines.Map(parser);
    }
}