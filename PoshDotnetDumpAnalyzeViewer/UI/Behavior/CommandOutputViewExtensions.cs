using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.UI.Behavior;

public static class CommandOutputViewExtensions
{
    public static T AddDefaultBehavior<T>(this T @this, IClipboard clipboard) where T : CommandOutputView
    {
        @this.ListView
            .AddClipboard(clipboard)
            .LinkWithFilterField(@this.Filter, (line, filter) => line.Contains(filter, StringComparison.OrdinalIgnoreCase))
            .ClipNavigationDown();

        @this.Filter.AddClipboard(clipboard);

        return @this;
    }
}