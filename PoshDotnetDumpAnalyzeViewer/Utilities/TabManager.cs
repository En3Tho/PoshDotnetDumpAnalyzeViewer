using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.Utilities;

// v2 notes:
// change TabView.Tab to Tab
public class TabManager(TabView tabView)
{
    private readonly Dictionary<string, (TabView.Tab Tab, bool IsOk)> _tabMap =
        new(StringComparer.OrdinalIgnoreCase);

    public (TabView.Tab Tab, bool IsOk)? TryGetTab(string command)
    {
        if (_tabMap.TryGetValue(command, out var result)) return result;
        return default((TabView.Tab Tab, bool IsOk)?);
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
        var command = TryGetCommand(tab);
        tabView.RemoveTab(tab);

        if (command is {})
        {
            _tabMap.Remove(command);
        }
    }

    public void RemoveTab(string command)
    {
        if (_tabMap.TryGetValue(command, out var result))
        {
            tabView.RemoveTab(result.Tab);
        }

        _tabMap.Remove(command);
    }

    public void AddTab(string command, TabView.Tab tab, bool isOk)
    {
        RemoveTab(command);
        _tabMap[command] = (tab, isOk);
        tabView.AddTab(tab, true);
    }

    public void SetSelected(TabView.Tab tab)
    {
        tabView.SelectedTab = tab;
    }
}