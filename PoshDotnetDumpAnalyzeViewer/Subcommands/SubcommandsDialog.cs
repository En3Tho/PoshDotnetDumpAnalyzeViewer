using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using PoshDotnetDumpAnalyzeViewer.Views;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.Subcommands;

public static class SubcommandsDialog
{
    public static SubcommandButton MakeButton(Toplevel parent, SubcommandsPriority priority, string title, Action onEnter, Action? onTab = null)
    {
        var button = new SubcommandButton
        {
            X = 0,
            Y = 0,
            Title = title
        };

        button.KeyDown += (_, args) =>
        {
            switch (args.KeyCode)
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

        var buttonFactory = new SubcommandButtonFactory(buttonsContainer, mainLayout, clipboard, commandQueue);
        buttonFactory.AddFrom(line);
        buttonsContainer.AddButtons(customButtonsFactory(buttonFactory));

        if (buttonsContainer.IsEmpty)
            return null;

        return buttonsContainer;
    }
}