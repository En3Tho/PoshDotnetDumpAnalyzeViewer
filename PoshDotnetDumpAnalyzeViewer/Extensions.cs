using En3Tho.ValueTupleExtensions.Linq;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

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
                if (clipboard.GetClipboardData() is {} clipboardData)
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
                if (historyList.PreviousCommand() is { } previousCommand)
                    @this.Text = previousCommand;
                args.Handled = true;
            }
            else if (args.KeyEvent.Key == nextCommandKey)
            {
                @this.Text = historyList.NextCommand() ?? "";
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

public static class ListViewExtensions
{
    public static T? GetSource<T>(this ListView @this)
        where T : class
    {
        return @this.Source.ToList() as T;
    }

    public static TOutputSpeciality? GetSelectedOutput<TOutputSpeciality>(this ListView @this)
        where TOutputSpeciality : class
    {
        var selectedItem = @this.SelectedItem;
        if (@this.GetSource<IList<OutputLine>>() is { } source && selectedItem >= 0)
            return source[selectedItem] as TOutputSpeciality;

        return default;
    }
}

public static class StringExtensions
{
    public static Range FindColumnRange(this string header, string columnName, Range previousColumnRange = default, bool isLastColumn = false)
    {
        var rangeStart = previousColumnRange.End.Value == 0 ? 0 : previousColumnRange.End.Value + 1;
        return isLastColumn
            ? Range.StartAt(rangeStart)
            : new(rangeStart, header.IndexOf(columnName, StringComparison.Ordinal) + columnName.Length);
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

public static class ArrayExtensions
{
    public static int IndexAfter<T>(this T[] @this, T value) where T : IEquatable<T>
    {
        var indexOfValue = @this.AsSpan().IndexOf(value);
        if (indexOfValue == -1 || indexOfValue == @this.Length - 1) return -1;
        return indexOfValue + 1;
    }

    public static int IndexAfter<T>(this T[] @this, Func<T, bool> finder)
    {
        // length - 1 because we don't need last element
        for (var i = 0; i < @this.Length - 1; i++)
        {
            if (finder(@this[i])) return i + 1;
        }

        return -1;
    }

    public static int IndexBefore<T>(this T[] @this, T value) where T : IEquatable<T>
    {
        var indexOfValue = @this.AsSpan().LastIndexOf(value);
        if (indexOfValue <= 0) return -1;
        return indexOfValue - 1;
    }

    public static int IndexBefore<T>(this T[] @this, Func<T, bool> finder)
    {
        for (var i = @this.Length -1; i > 1; i --)
        {
            if (finder(@this[i])) return i - 1;
        }

        return -1;
    }

    public static T[] TakeAfter<T>(this T[] @this, T value) where T : IEquatable<T>
    {
        var start = @this.IndexAfter(value);
        if (start == -1) return Array.Empty<T>();
        return @this[start..];
    }

    public static T[] TakeAfter<T>(this T[] @this, Func<T, bool> finder) where T : IEquatable<T>
    {
        var start = @this.IndexAfter(finder);
        if (start == -1) return Array.Empty<T>();
        return @this[start..];
    }

    public static T[] TakeBetween<T>(this T[] @this, Func<T, bool> finder1, Func<T, bool> finder2) where T : IEquatable<T>
    {
        var start = @this.IndexAfter(finder1);
        if (start == -1) return Array.Empty<T>();

        var end = @this.IndexBefore(finder2);
        if (end == -1 || end <= start) return Array.Empty<T>();
        return @this[start..end];
    }

    public static T[] TakeBetween<T>(this T[] @this, T first, T last) where T : IEquatable<T>
    {
        var start = @this.IndexAfter(first);
        if (start == -1) return Array.Empty<T>();

        var end = @this.IndexBefore(last);
        if (end == -1 || end <= start) return Array.Empty<T>();
        return @this[start..end];
    }

    private static void Map<TIn, TOut>(this Span<TIn> @this, Span<TOut> result, Func<TIn, TOut> mapper)
    {
        if (@this.Length != result.Length)
            throw new ArgumentException("Span's lengths are different");

        for (var i = 0; i < @this.Length; i++)
            result[i] = mapper(@this[i]);
    }

    public static TOut[] Map<TIn, TOut>(this TIn[] @this, Func<TIn, TOut> mapper)
    {
        var result = new TOut[@this.Length];
        @this.AsSpan().Map(result.AsSpan(), mapper);
        return result;
    }

    public static TOut[] MapRange<TIn, TOut>(this TIn[] @this, Func<TIn, TOut> defaultMapper, params RangeMapper<TIn, TOut>[] mappers)
    {
        var result = new TOut[@this.Length];

        // sort ranges and filter empty ones
        mappers = mappers.OrderBy(x => x.Range.Start.Value).Where(x => x.Range.GetOffsetAndLength(@this.Length).Length != 0).ToArray();

        if (mappers.Length == 0)
        {
            @this.AsSpan().Map(result.AsSpan(), defaultMapper);
        }
        else
        {
            var startingRange = Range.EndAt(mappers[0].Range.Start);
            @this.AsSpan(startingRange).Map(result.AsSpan(startingRange), defaultMapper);

            if (mappers.Length == 1)
            {
                var mapper = mappers[0];
                @this.AsSpan(mapper.Range).Map(result.AsSpan(mapper.Range), x => mapper.Map(x));
            }
            else
            {
                foreach (var (prev, current) in mappers.Pairwise())
                {
                    @this.AsSpan(prev.Range).Map(result.AsSpan(prev.Range), x => prev.Map(x));

                    var betweenRange = new Range(prev.Range.End, current.Range.Start);
                    @this.AsSpan(betweenRange).Map(result.AsSpan(betweenRange), defaultMapper);

                    @this.AsSpan(current.Range).Map(result.AsSpan(current.Range), x => current.Map(x));
                }
            }

            var endingRange = Range.StartAt(mappers[^1].Range.End);
            @this.AsSpan(endingRange).Map(result.AsSpan(endingRange), defaultMapper);
        }


        return result;
    }
}

public static class EnumerableExtensions
{
    public static IEnumerable<T> TakeAfter<T>(this IEnumerable<T> @this, T value) where T : IEquatable<T>
        => @this.SkipWhile(element => !value.Equals(element)).Skip(1);
}