using PoshDotnetDumpAnalyzeViewer.UI.Subcommands;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.UI;

public class SubcommandButton : Button
{
    public SubcommandsPriority Priority { get; set; }
}

public class SubcommandsView(IEnumerable<SubcommandButton> buttons) : Window
{
    private readonly List<SubcommandButton> _buttons = buttons.ToList();
    private int maxButtonLength;

    private void Resize()
    {
        RemoveAll();

        var i = 0;
        foreach (var button in _buttons.OrderBy(x => x.Priority))
        {
            button.Y = i++;
            Add(button);
        }

        Width = maxButtonLength + 6;
        Height = _buttons.Count + 2;
    }

    public void AddButton(SubcommandButton button)
    {
        _buttons.Add(button);
        if (maxButtonLength < button.Text.Length)
        {
            maxButtonLength = button.Text.Length;
        }
    }

    public void AddButtons(IEnumerable<SubcommandButton> buttons)
    {
        foreach (var button in buttons)
        {
            AddButton(button);
        }
        Resize();
    }

    public bool IsEmpty => _buttons.Count == 0;
}