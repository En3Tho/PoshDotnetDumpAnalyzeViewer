using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.Utilities;

public class TabManager(TabView tabView)
{
    private readonly Dictionary<string, (Tab Tab, View View, bool IsOk)> _tabMap =
        new(StringComparer.OrdinalIgnoreCase);

    public (Tab Tab, View View, bool IsOk)? TryGetTab(string command)
    {
        if (_tabMap.TryGetValue(command, out var result)) return result;
        return default;
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

        if (command is { })
            RemoveTab(command);
    }

    public void RemoveTab(string command)
    {
        if (_tabMap.TryGetValue(command, out var result))
        {
            tabView.RemoveTab(result.Tab);
        }

        _tabMap.Remove(command);
    }

    public void AddTab(string command, View view, Tab tab, bool isOk)
    {
        RemoveTab(command);
        _tabMap[command] = (tab, view, isOk);
        tabView.AddTab(tab, true);
    }

    public void SetSelected(Tab tab)
    {
        tabView.SelectedTab = tab;
    }
}