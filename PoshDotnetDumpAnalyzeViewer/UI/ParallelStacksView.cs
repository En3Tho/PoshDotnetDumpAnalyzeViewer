using PoshDotnetDumpAnalyzeViewer.Parsing;

namespace PoshDotnetDumpAnalyzeViewer.UI;

public class ParallelStacksView(string[] data) : CommandOutputView(data)
{
    public bool Shrinked { get; private set; }

    public void Shrink()
    {
        if (Shrinked)
            return;

        var output = ParallelStacksParser.ShrinkParallelStacksOutput(ListView.Source);
        ListView.SetSource(output);
        Shrinked = true;
    }

    public void Restore()
    {
        if (!Shrinked)
            return;

        ListView.SetSource(ListView.InitialSource);
        Shrinked = false;
    }
}