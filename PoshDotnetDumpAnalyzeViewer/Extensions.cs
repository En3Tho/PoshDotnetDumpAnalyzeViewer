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
    public static T GetSource<T>(this ListView @this)
    {
        return (T)@this.Source.ToList();
    }

    public static T GetSelectedItem<T>(this ListView @this)
    {
        return @this.GetSource<IList<T>>()[@this.SelectedItem];
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

    public static int IndexBefore<T>(this T[] @this, T value) where T : IEquatable<T>
    {
        var indexOfValue = @this.AsSpan().LastIndexOf(value);
        if (indexOfValue <= 0) return -1;
        return indexOfValue - 1;
    }

    public static T[] TakeAfter<T>(this T[] @this, T value) where T : IEquatable<T>
    {
        var start = @this.IndexAfter(value);
        if (start == -1) return Array.Empty<T>();
        return @this[start..];
    }

    public static T[] TakeBetween<T>(this T[] @this, T first, T last) where T : IEquatable<T>
    {
        var start = @this.IndexAfter(first);
        if (start == -1) return Array.Empty<T>();

        var end = @this.IndexBefore(last);
        if (end == -1 || end <= start) return Array.Empty<T>();
        return @this[start..end];
    }
}

public static class EnumerableExtensions
{
    public static IEnumerable<T> TakeAfter<T>(this IEnumerable<T> @this, T value) where T : IEquatable<T>
        => @this.SkipWhile(element => !value.Equals(element)).Skip(1);
}