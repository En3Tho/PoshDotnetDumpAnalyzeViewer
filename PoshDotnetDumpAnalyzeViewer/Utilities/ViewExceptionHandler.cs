using PoshDotnetDumpAnalyzeViewer.UI;
using PoshDotnetDumpAnalyzeViewer.UI.Behavior;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.Utilities;

public class ViewExceptionHandler
{
    public static Func<Exception, bool> Create(TabManager tabManager, IClipboard clipboard)
    {
        return exn =>
        {
            var errorSource =
                exn.ToString()
                    .Split(Environment.NewLine);

            var commandView = new CommandOutputView(errorSource).AddDefaultBehavior(clipboard);

            var tab = new TabView.Tab
            {
                Text = "Unhandled exception",
                View = commandView
            };
            tabManager.AddTab(exn.Message, tab, false);

            return true;
        };
    }
}