using PoshDotnetDumpAnalyzeViewer.UI;
using PoshDotnetDumpAnalyzeViewer.UI.Extensions;
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

            var tab = new Tab
            {
                Title = "Unhandled exception",
                View = commandView
            };
            tabManager.AddTab(exn.Message, commandView, tab, false);

            return true;
        };
    }
}