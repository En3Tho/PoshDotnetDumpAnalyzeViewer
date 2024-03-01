using PoshDotnetDumpAnalyzeViewer.Interactivity;
using PoshDotnetDumpAnalyzeViewer.Views;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public class UI
{
    public static Func<Exception, bool> MakeExceptionHandler(TabManager tabManager, IClipboard clipboard)
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