using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.UI.Subcommands;

using KeyCode = Key;

// v2 notes: args to (_, args)
// Remove KeyCode
// Text to Title or something like that
// KeyEvent to KeyCode

public static class SubcommandsDialog
{
    public static SubcommandButton MakeButton(Toplevel parent, SubcommandsPriority priority, string title, Action onEnter, Action? onTab = null)
    {
        var button = new SubcommandButton
        {
            X = 0,
            Y = 0,
            Text = title
        };

        button.KeyDown += args =>
        {
            switch (args.KeyEvent.Key)
            {
                case KeyCode.Tab:
                    if (onTab is { })
                    {
                        Application.RequestStop(parent);
                        onTab();
                        args.Handled = true;
                    }
                    break;
                case KeyCode.Enter:
                    Application.RequestStop(parent);
                    onEnter();
                    args.Handled = true;
                    break;
                case KeyCode.Esc:
                    Application.RequestStop(parent);
                    break;
            }
        };

        return button;
    }

    public static Toplevel? TryCreate(
        MainLayout mainLayout,
        OutputLine line,
        Func<SubcommandButtonFactory, IEnumerable<SubcommandButton>> customButtonsFactory,
        IClipboard clipboard,
        CommandQueue commandQueue)
    {
        var buttonsContainer = new SubcommandsView([])
        {
            Title = "Available commands",
        };

        buttonsContainer.KeyDown += args =>
        {
            switch (args.KeyEvent.Key)
            {
                case KeyCode.CursorUp:
                    buttonsContainer.ProcessKey(new(KeyCode.CursorLeft, new()));
                    args.Handled = true;
                    break;
                case KeyCode.CursorDown:
                    buttonsContainer.ProcessKey(new(KeyCode.CursorRight, new()));
                    args.Handled = true;
                    break;
            }
        };

        var buttonFactory = new SubcommandButtonFactory(buttonsContainer, mainLayout, clipboard, commandQueue);
        buttonFactory.AddFrom(line);
        buttonsContainer.AddButtons(customButtonsFactory(buttonFactory));

        if (buttonsContainer.IsEmpty)
            return null;

        return buttonsContainer;
    }
}