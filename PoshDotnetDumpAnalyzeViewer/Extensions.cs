using System.Text.RegularExpressions;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public static class TextFieldExtensions
{
    public static TextField AddClipboard(this TextField @this, IClipboard clipboard,
        KeyCode copyKey = KeyCode.CtrlMask | KeyCode.C, KeyCode pasteKey = KeyCode.CtrlMask | KeyCode.V)
    {
        @this.KeyDown += (_, key) =>
        {
            if (key.KeyCode == copyKey)
            {
                @this.Copy(clipboard);
                key.Handled = true;
            }
            else if (key.KeyCode == pasteKey)
            {
                if (clipboard.GetClipboardData() is { } clipboardData)
                    @this.Paste(clipboardData);
                key.Handled = true;
            }
        };

        return @this;
    }

    public static TextField AddCommandHistory(this TextField @this, HistoryList<string> historyList,
        KeyCode previousCommandKey = KeyCode.CursorUp, KeyCode nextCommandKey = KeyCode.CursorDown)
    {
        @this.KeyDown += (_, key) =>
        {
            if (key.KeyCode == previousCommandKey)
            {
                if (historyList.Previous() is { } previousCommand)
                    @this.Text = previousCommand;

                key.Handled = true;
            }
            else if (key.KeyCode == nextCommandKey)
            {
                @this.Text = historyList.Next() ?? "";
                key.Handled = true;
            }
        };

        return @this;
    }

    public static void Copy(this TextField @this, IClipboard clipboard)
    {
        clipboard.SetClipboardData(@this.SelectedText is { Length: > 0 }
            ? @this.SelectedText
            : @this.Text);
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
            var originalText = @this.Text;
            var result = string.Create(originalText.Length - start + text.Length, (originalText, start, text), (span, state) =>
            {
                state.originalText.AsSpan(0, state.start).CopyTo(span);
                state.text.AsSpan().CopyTo(span[state.start..]);
                state.originalText.AsSpan(state.start + @this.SelectedLength).CopyTo(span[(state.start + state.text.Length)..]);
            });

            @this.Text = result;
            @this.ClearAllSelection();
            @this.CursorPosition = start + text.Length;
        }
        else
        {
            var originalPosition = @this.CursorPosition;
            var originalText = @this.Text;
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
                : new(@this.Index, @this.Index + @this.Length);
    }
}

public static class MatchExtensions
{
    public static void CopyGroupsRangesTo(this Match @this, Span<Range> ranges)
    {
        var i = 0;
        foreach (var group in @this.Groups.Cast<Group>().Skip(1).Take(ranges.Length))
        {
            var range = group.GetRange();
            ranges[i++] = range;
        }

        if (i != ranges.Length)
            throw new ArgumentException("Ranges length is different from match groups count");
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
    public static void Map<TIn, TOut>(this ReadOnlySpan<TIn> @this, Span<TOut> result, Func<TIn, TOut> mapper)
    {
        if (@this.Length != result.Length)
            throw new ArgumentException("Span's lengths are different");

        for (var i = 0; i < @this.Length; i++)
            result[i] = mapper(@this[i]);
    }

    public static void Map<TIn, TOut>(this Span<TIn> @this, Span<TOut> result, Func<TIn, TOut> mapper) =>
        Map((ReadOnlySpan<TIn>)@this, result, mapper);
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