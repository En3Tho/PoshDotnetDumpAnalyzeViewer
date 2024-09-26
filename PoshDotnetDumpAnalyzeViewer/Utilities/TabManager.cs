using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.Utilities;

public class TabManager(TabView tabView)
{
    private readonly Dictionary<string, (Tab Tab, bool IsOk)> _tabMap =
        new(StringComparer.OrdinalIgnoreCase);

    public (Tab Tab, bool IsOk)? TryGetTab(string command)
    {
        if (_tabMap.TryGetValue(command, out var result)) return result;
        return default((Tab Tab, bool IsOk)?);
    }

    public string? TryGetCommand(Tab tab)
    {
        foreach (var commandsAndKeys in _tabMap)
        {
            if (ReferenceEquals(commandsAndKeys.Value.Tab, tab))
                return commandsAndKeys.Key;
        }

        return null;
    }

    public void RemoveTab(Tab tab)
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

    public void AddTab(string command, Tab tab, bool isOk)
    {
        RemoveTab(command);
        _tabMap[command] = (tab, isOk);
        tabView.AddTab(tab, true);
    }

    public void SetSelected(Tab tab)
    {
        tabView.SelectedTab = tab;
    }
}