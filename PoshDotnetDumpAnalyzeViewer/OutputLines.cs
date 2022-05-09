namespace PoshDotnetDumpAnalyzeViewer;

public record OutputLine(string Line) : IOutputLine<OutputLine>
{
    public override string ToString() => Line;
}

public sealed record HelpOutputLine(string Line) : OutputLine(Line), IOutputLine<HelpOutputLine>, IHelpCommand
{
    public override string ToString() => Line;
    public string[] Commands => Parser.Help.GetCommandsFromLine(Line);
}