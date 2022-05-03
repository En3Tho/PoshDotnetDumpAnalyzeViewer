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
    public static void Copy(this TextField @this, MiniClipboard clipboard)
    {
        clipboard.Set(@this.SelectedText is { Length: > 0 }
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