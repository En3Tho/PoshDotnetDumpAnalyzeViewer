using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public record MiniClipboard(IClipboard Clipboard) : IClipboard
{
    private string? _clipboard;

    public string? GetClipboardData()
    {
        TryGetClipboardData(out var result);
        return result;
    }

    public bool TryGetClipboardData(out string? result)
    {
        if (!Clipboard.TryGetClipboardData(out result))
            result = _clipboard;
        return true;
    }

    public void SetClipboardData(string? text)
    {
        var trimmed = text?.Trim();
        Clipboard.TrySetClipboardData(trimmed);
        _clipboard = trimmed;
    }

    public bool TrySetClipboardData(string? text)
    {
        SetClipboardData(text);
        return true;
    }

    public bool IsSupported => true;
}

public class HistoryList<T>
{
    private readonly List<T> _items = new();
    private int _currentIndex;

    private void RemoveCommand(T item)
    {
        if (_items.IndexOf(item) is >= 0 and var idx)
            _items.RemoveAt(idx);
    }

    private T? GetCurrentCommand()
    {
        if (_currentIndex >= 0 && _currentIndex < _items.Count)
            return _items[_currentIndex];
        return default;
    }

    public void AddCommand(T command)
    {
        RemoveCommand(command);

        _items.Add(command);
        _currentIndex = _items.Count;
    }

    public T? PreviousCommand()
    {
        _currentIndex = Math.Max(0, _currentIndex - 1);
        return GetCurrentCommand();
    }

    public T? NextCommand()
    {
        _currentIndex = Math.Min(_currentIndex + 1, _items.Count);
        return GetCurrentCommand();
    }
}

public record struct RangeMapper<TIn, TOut>(Range Range, Func<TIn, TOut> Map);

public interface IViewWithTab
{
    public TabView.Tab Tab { get; }
}

public class TabManager
{
    private readonly Dictionary<string, TabView.Tab> _tabMap =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly MainLoop _loop;
    private readonly TabView _tabView;

    public TabManager(MainLoop loop, TabView tabView)
    {
        _loop = loop;
        _tabView = tabView;
    }

    public TabView.Tab? TryGetTab(string command)
    {
        if (_tabMap.TryGetValue(command, out var result)) return result;
        return default;
    }

    public string? TryGetCommand(TabView.Tab tab)
    {
        foreach (var commandsAndKeys in _tabMap)
        {
            if (ReferenceEquals(commandsAndKeys.Value, tab))
                return commandsAndKeys.Key;
        }

        return null;
    }

    public void RemoveTab(TabView.Tab tab)
    {
        string? command = TryGetCommand(tab);

        if (command is { })
            RemoveTab(command);
    }

    public void RemoveTab(string command)
    {
        if (_tabMap.TryGetValue(command, out var result))
        {
            _loop.Invoke(() => { _tabView.RemoveTab(result); });
        }

        _tabMap.Remove(command);
    }

    public void SetTab(string command, TabView.Tab result)
    {
        RemoveTab(command);

        _tabMap[command] = result;
        _loop.Invoke(() => { _tabView.AddTab(result, true); });
    }

    public bool TrySetSelectedExistingTab(string command)
    {
        if (TryGetTab(command) is { } existingTab)
        {
            SetSelected(existingTab);
            return true;
        }

        return false;
    }

    public void SetSelected(TabView.Tab tab) => _loop.Invoke(() => _tabView.SelectedTab = tab);
}