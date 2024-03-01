using PoshDotnetDumpAnalyzeViewer.Utilities;
using PoshDotnetDumpAnalyzeViewer.Views;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.ViewBehavior;

public static class MainLayoutExtensions
{
    public static MainLayout AddDefaultBehavior(this MainLayout @this, TabManager tabManager, CommandQueue commandQueue, IClipboard clipboard,
        HistoryList<string> commandHistory)
    {
        @this.TabView.KeyDown += (_, args) =>
        {
            switch (args.KeyCode)
            {
                case KeyCode.CtrlMask | KeyCode.W:
                {
                    // special case help
                    if (@this.TabView is { SelectedTab: { Text: not "help" } selectedTab})
                        tabManager.RemoveTab(selectedTab);
                    args.Handled = true;
                    break;
                }
                case KeyCode.CtrlMask | KeyCode.R:
                {
                    // special case help
                    if (@this.TabView is { SelectedTab.Text: not "help" and var tabText})
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

        void ProcessEnterKey(bool forceRefresh)
        {
            var command = @this.CommandInput.Text;
            if (string.IsNullOrWhiteSpace(command)) return;
            commandQueue.SendCommand(command, forceRefresh: forceRefresh);
        }

        @this.CommandInput
            .AddClipboard(clipboard)
            .AddCommandHistory(commandHistory)
            .KeyDown += (_, args) =>
        {
            switch (args.KeyCode)
            {
                case KeyCode.CtrlMask | KeyCode.Enter:
                    ProcessEnterKey(true);
                    args.Handled = true;
                    break;
                case KeyCode.Enter:
                    ProcessEnterKey(false);
                    args.Handled = true;
                    break;
            }
        };

        return @this;
    }
}