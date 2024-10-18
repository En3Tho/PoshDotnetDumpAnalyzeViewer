using PoshDotnetDumpAnalyzeViewer.UI.Behavior;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.UI;

// v2 notes Dim.Sized => Dim.Absolute

public class MainLayout : Window
{
    public TabView TabView { get; }
    public TextField CommandInput { get; }

    public MainLayout()
    {
        Width = Dim.Fill();
        Height = Dim.Fill();

        TabView = new()
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()! - Dim.Sized(3)!
        };

        var commandFrame = new FrameView
        {
            Title = "Command",
            Y = Pos.Bottom(TabView),
            Height = 3,
            Width = Dim.Fill()
        };

        CommandInput = new()
        {
            Width = Dim.Fill(),
            Height = 1
        };

        this.With(
            TabView,
            commandFrame.With(
                CommandInput));
    }
}