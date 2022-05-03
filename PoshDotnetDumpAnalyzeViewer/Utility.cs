using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public record MiniClipboard(IClipboard Clipboard)
{
    private string? _clipboard;

    public void Set(string? value)
    {
        var trimmed = value?.Trim();
        Clipboard.TrySetClipboardData(trimmed);
        _clipboard = trimmed;
    }

    public string? Get()
    {
        if (Clipboard.TryGetClipboardData(out var result))
            return result;
        return _clipboard;
    }
}

public class CommandHistory
{
    private readonly List<string> _commands = new();
    private int _currentIndex = 0;

    private void RemoveCommand(string command)
    {
        if (_commands.IndexOf(command) is >= 0 and var idx)
            _commands.RemoveAt(idx);
    }

    private string? GetCurrentCommand()
    {
        if (_currentIndex >= 0 && _currentIndex < _commands.Count)
            return _commands[_currentIndex];
        return null;
    }
    
    public void AddCommand(string command)
    {
        RemoveCommand(command);
        
        _commands.Add(command);
        _currentIndex = _commands.Count;
    }

    public string? PreviousCommand()
    {
        _currentIndex = Math.Max(-1, _currentIndex - 1);
        return GetCurrentCommand();
    }

    public string? NextCommand()
    {
        _currentIndex = Math.Min(_currentIndex + 1, _commands.Count);
        return GetCurrentCommand();
    }
}

public class TabManager
{
    readonly Dictionary<string, (CommandViews Views, CommandOutput Output)> _tabMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly TabView _tabView;

    public TabManager(TabView tabView) => _tabView = tabView;

    public (CommandViews Views, CommandOutput Output)? TryGetTab(string command)
    {
        if (_tabMap.TryGetValue(command, out var result)) return result;
        return default;
    }

    public string? TryGetCommand(TabView.Tab tab)
    {
        foreach (var commandsAndKeys in _tabMap)
        {
            if (ReferenceEquals(commandsAndKeys.Value.Views.Tab, tab))
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
            _tabView.RemoveTab(result.Views.Tab);
        }
        _tabMap.Remove(command);
    }

    public void SetTab(string command, (CommandViews Views, CommandOutput Output) result)
    {
        RemoveTab(command);

        _tabMap[command] = result;
        _tabView.AddTab(result.Views.Tab, true);
    }
}