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

    private void Remove(T item)
    {
        if (_items.IndexOf(item) is >= 0 and var idx)
            _items.RemoveAt(idx);
    }

    private T? GetCurrent()
    {
        if (_currentIndex >= 0 && _currentIndex < _items.Count)
            return _items[_currentIndex];
        return default;
    }

    public void Add(T command)
    {
        Remove(command);

        _items.Add(command);
        _currentIndex = _items.Count;
    }

    public T? Previous()
    {
        _currentIndex = Math.Max(0, _currentIndex - 1);
        return GetCurrent();
    }

    public T? Next()
    {
        _currentIndex = Math.Min(_currentIndex + 1, _items.Count);
        return GetCurrent();
    }
}

public record struct RangeMapper<TIn, TOut>(Range Range, Func<TIn, TOut> Map);

public class TabManager
{
    private readonly Dictionary<string, (TabView.Tab Tab, CommandOutputViews Views, bool IsOk)> _tabMap =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly TabView _tabView;

    public TabManager(TabView tabView)
    {
        _tabView = tabView;
    }

    public (TabView.Tab Tab, CommandOutputViews Views, bool IsOk)? TryGetTab(string command)
    {
        if (_tabMap.TryGetValue(command, out var result)) return result;
        return default;
    }

    public string? TryGetCommand(TabView.Tab tab)
    {
        foreach (var commandsAndKeys in _tabMap)
        {
            if (ReferenceEquals(commandsAndKeys.Value.Tab, tab))
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
            //_loop.Invoke(() => { _tabView.RemoveTab(result.Tab); });
            _tabView.RemoveTab(result.Tab);
        }

        _tabMap.Remove(command);
    }

    public void AddTab(string command, CommandOutputViews views, TabView.Tab tab, bool isOk)
    {
        RemoveTab(command);

        _tabMap[command] = (tab, views, isOk);
        //_loop.Invoke(() => { _tabView.AddTab(tab, true); });
        _tabView.AddTab(tab, true);
    }

    public void SetSelected(TabView.Tab tab)
    {
        //_loop.Invoke(() => _tabView.SelectedTab = tab);
        _tabView.SelectedTab = tab;
    }
}