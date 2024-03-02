using PoshDotnetDumpAnalyzeViewer.Views;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.ViewBehavior;

public static class CommandOutputViewExtensions
{
    public static T AddDefaultBehavior<T>(this T @this, IClipboard clipboard) where T : CommandOutputView
    {
        @this.ListView
            .AddClipboard(clipboard)
            .LinkWithFilterField(@this.Filter, (line, filter) => line.Contains(filter, StringComparison.OrdinalIgnoreCase))
            .FixNavigationDown();

        @this.Filter.AddClipboard(clipboard);

        return @this;
    }
}