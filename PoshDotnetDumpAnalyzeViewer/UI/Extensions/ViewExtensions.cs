using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.UI.Extensions;

public static class ViewExtensions
{
    public static T With<T>(this T @this, View view, params View[] views) where T : View
    {
        @this.Add(view);
        foreach (var nextView in views)
        {
            @this.Add(nextView);
        }

        return @this;
    }
}