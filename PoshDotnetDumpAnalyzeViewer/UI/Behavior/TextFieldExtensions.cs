using PoshDotnetDumpAnalyzeViewer.Utilities;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.UI.Behavior;

using KeyCode = Key;

// v2 notes: remove KeyCode
// change args to (_, key) =>
// remove ToString calls from @this.Text and other properties
public static class TextFieldExtensions
{
    public static TextField AddClipboard(this TextField @this, IClipboard clipboard,
        KeyCode copyKey = KeyCode.CtrlMask | KeyCode.C, KeyCode pasteKey = KeyCode.CtrlMask | KeyCode.V)
    {
        @this.KeyUp += args =>
        {
            var key = args.KeyEvent.Key;

            if (key == copyKey)
            {
                @this.Copy(clipboard);
                args.Handled = true;
            }
            else if (key == pasteKey)
            {
                if (clipboard.GetClipboardData() is { } clipboardData)
                    @this.Paste(clipboardData);
                args.Handled = true;
            }
        };

        return @this;
    }

    public static TextField AddCommandHistory(this TextField @this, HistoryList<string> historyList,
        KeyCode previousCommandKey = KeyCode.CursorUp, KeyCode nextCommandKey = KeyCode.CursorDown)
    {
        @this.KeyDown += args =>
        {
            var key = args.KeyEvent.Key;
            if (key == previousCommandKey)
            {
                if (historyList.Previous() is { } previousCommand)
                    @this.Text = previousCommand;

                args.Handled = true;
            }
            else if (key == nextCommandKey)
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
            : @this.Text.ToString());
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
                state.originalText.ToString().AsSpan(0, state.start).CopyTo(span);
                state.text.AsSpan().CopyTo(span[state.start..]);
                state.originalText.ToString().AsSpan(state.start + @this.SelectedLength).CopyTo(span[(state.start + state.text.Length)..]);
            });

            @this.Text = result;
            @this.ClearAllSelection();
            @this.CursorPosition = start + text.Length;
        }
        else
        {
            var originalPosition = @this.CursorPosition;
            var originalText = @this.Text;
            @this.Text = originalText.ToString()!.Insert(originalPosition, text);
            @this.CursorPosition = originalPosition + text.Length;
        }
    }
}