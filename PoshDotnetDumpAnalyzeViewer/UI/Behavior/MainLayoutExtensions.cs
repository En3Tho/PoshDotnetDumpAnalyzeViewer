using PoshDotnetDumpAnalyzeViewer.Utilities;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.UI.Behavior;

using KeyCode = Key;

// v2 notes: args to (_, args)
// Remove KeyCode
// Text to Title or something like that
// KeyEvent to KeyCode

public static class MainLayoutExtensions
{
    public static MainLayout AddDefaultBehavior(this MainLayout @this, TabManager tabManager, CommandQueue commandQueue, IClipboard clipboard,
        HistoryList<string> commandHistory)
    {
        @this.TabView.KeyDown += args =>
        {
            switch (args.KeyEvent.Key)
            {
                case KeyCode.CtrlMask | KeyCode.W:
                {
                    // special case help
                    if (@this.TabView is { SelectedTab: { Text: {} tabText } selectedTab} && tabText?.ToString() is not "help")
                        tabManager.RemoveTab(selectedTab);
                    args.Handled = true;
                    break;
                }
                case KeyCode.CtrlMask | KeyCode.R:
                {
                    // special case help
                    if (@this.TabView is { SelectedTab.Text: {} tabTextU } && tabTextU.ToString()! is not "help" and var tabText)
                    {
                        if (tabManager.TryGetTab(tabText) is { IsOk: true })
                        {
                            commandQueue.SendCommand(tabText, forceRefresh: true);
                        }
                    }
                    args.Handled = true;
                    break;
                }
            }
        };

        void ProcessEnterKey(bool forceRefresh = false)
        {
            var command = @this.CommandInput.Text?.ToString();
            if (string.IsNullOrWhiteSpace(command)) return;
            commandQueue.SendCommand(command, forceRefresh: forceRefresh);
        }

        @this.CommandInput
            .AddClipboard(clipboard)
            .AddCommandHistory(commandHistory)
            .KeyDown += args =>
        {
            switch (args.KeyEvent.Key)
            {
                case KeyCode.CtrlMask | KeyCode.Enter:
                    if (!@this.CommandInput.ReadOnly)
                    {
                        ProcessEnterKey(true);
                    }
                    args.Handled = true;
                    break;
                case KeyCode.Enter:
                    if (!@this.CommandInput.ReadOnly)
                    {
                        ProcessEnterKey();
                    }
                    args.Handled = true;
                    break;
            }
        };

        return @this;
    }
}