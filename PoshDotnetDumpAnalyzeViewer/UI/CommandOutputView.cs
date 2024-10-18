using PoshDotnetDumpAnalyzeViewer.UI.Behavior;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.UI;

// v2 notes Dim.Sized => Dim.Absolute

public class CommandOutputView : Window
{
    public ArrayListView<string> ListView { get; }
    public TextField Filter { get; }

    public CommandOutputView(string[] data)
    {
        Width = Dim.Fill();
        Height = Dim.Fill();

        ListView = new(data)
        {
            Height = Dim.Fill()! - Dim.Sized(2)!,
            Width = Dim.Fill()
        };

        var filterFrame = new FrameView
        {
            Title = "Filter",
            Y = Pos.Bottom(ListView),
            Height = 3,
            Width = Dim.Fill()
        };

        Filter = new()
        {
            Height = 1,
            Width = Dim.Fill()
        };

        ListView.SetSource(data);

        this.With(
            ListView,
            filterFrame.With(
                Filter));
    }
}