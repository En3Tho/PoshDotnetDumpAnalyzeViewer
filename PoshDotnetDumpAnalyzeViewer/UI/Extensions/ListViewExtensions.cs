using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.UI.Extensions;

public static class ListViewExtensions
{
    public static ListView FixNavigationDown(this ListView @this)
    {
        @this.KeyDown += (_, args) =>
        {
            switch (args.KeyCode)
            {
                case KeyCode.End:
                    @this.SelectedItem = @this.Source.Length - 1;
                    @this.TopItem = Math.Max(0, @this.Source.Length - @this.Frame.Height);
                    args.Handled = true;
                    break;
                case KeyCode.PageDown:
                    var jumpSize = @this.Frame.Height;
                    var jumpPoint = @this.SelectedItem;
                    var jumpMax = @this.Source.Length - @this.Frame.Height;
                    if (jumpPoint + jumpSize > jumpMax)
                    {
                        @this.SelectedItem = Math.Min(@this.Source.Length - 1, jumpPoint + jumpSize);
                        @this.TopItem = Math.Max(0, jumpMax);
                        args.Handled = true;
                    }

                    break;
            }
        };
        return @this;
    }
}