namespace PoshDotnetDumpAnalyzeViewer;

public readonly record struct DefaultOutputLine(string Line) : IOutputLine<DefaultOutputLine>
{
    public override string ToString() => Line;

    public static DefaultOutputLine FromLine(string line) => new(line);
}