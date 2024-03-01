using PoshDotnetDumpAnalyzeViewer.Utilities;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.ViewBehavior;

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