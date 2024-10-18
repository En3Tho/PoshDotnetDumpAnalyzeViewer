using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.Utilities;

public class MiniClipboard(IClipboard clipboard) : IClipboard
{
    private string? _clipboardText;

    public string? GetClipboardData()
    {
        TryGetClipboardData(out var result);
        return result;
    }

    public bool TryGetClipboardData(out string? result)
    {
        if (!clipboard.TryGetClipboardData(out result))
            result = _clipboardText;
        return true;
    }

    public void SetClipboardData(string? text)
    {
        var trimmed = text?.Trim();
        clipboard.TrySetClipboardData(trimmed);
        _clipboardText = trimmed;
    }

    public bool TrySetClipboardData(string? text)
    {
        SetClipboardData(text);
        return true;
    }

    public bool IsSupported => true;
}