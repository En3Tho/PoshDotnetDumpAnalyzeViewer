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
                        args.Handled = true;
                        Application.RequestStop(parent);
                        onTab();
                    }
                    break;
                case KeyCode.Enter:
                    args.Handled = true;
                    Application.RequestStop(parent);
                    onEnter();
                    break;
                case KeyCode.Esc:
                    args.Handled = true;
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
                    args.Handled = true;
                    buttonsContainer.ProcessKey(new(KeyCode.CursorLeft, new()));
                    break;
                case KeyCode.CursorDown:
                    args.Handled = true;
                    buttonsContainer.ProcessKey(new(KeyCode.CursorRight, new()));
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