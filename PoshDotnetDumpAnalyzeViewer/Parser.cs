using System.Text.RegularExpressions;

namespace PoshDotnetDumpAnalyzeViewer;

static class RegexPatterns
{
    private const string D = @"-?[0-9]+";
    private const string H = @"(?:0[xX])?[0-9a-fA-F]+";
    private const string A = @"[0-9a-fA-F]{16}";
    private const string S = @"[^\s].*[^\s]";
    private const string C = @".+";
    private const string Co = @".*";
    private const string WS = @"\s+";
    private const string WSo = @"\s*";
    private const string Dg = $"({D})";
    private const string Hg = $"({H})";
    private const string Ag = $"({A})";
    private const string Sg = $"({S})";
    private const string Sgo = $"({S})*";

    // ...   OsId ...
    // C WS  Hg    WS C
    public const string OsId =
        $"{C}{Hg}{C}";

    //                                                                               Lock
    // "DBG           ID  OSID  ThreadOBJ  State  GC Mode  GC Alloc Context  Domain  Count  Apt   Exception"
    // "(D | "XXXX")g Dg  Hg    Ag         Hg     Sg       (A & ":" & A)g    Ag      Dg     Sg    Sg?
    public const string ClrThreads =
        $"{WSo}((?:XXXX)|{D}){WS}{Dg}{WS}{Hg}{WS}{Ag}{WS}{Hg}{WS}{Sg}{WS}({A}:{A}){WS}{Ag}{WS}{Dg}{WS}{Sg}{WSo}{Sgo}{WSo}";
}

public static class Help
{
    // TODO: regex, single-line parsing
    public static CommandOutput Parse(string command, string[] output)
    {
        var commandStartIndex = output.IndexAfter("Commands:");
        var helpCommandsRange = commandStartIndex..;

        return new(command, output.MapRange(
            x => new(x),
            new RangeMapper<string, OutputLine>(helpCommandsRange, x =>
            {
                if (string.IsNullOrWhiteSpace(x) || x.AsSpan()[..42].IsWhiteSpace())
                    return new(x);
                return new HelpOutputLine(x);
            })));
    }

    public static string[] GetCommandsFromLine(string line)
    {
        // usually there is 42 chars for commands column, next is description column
        return line[..42].Split(",", StringSplitOptions.TrimEntries)
            .Select(part =>
                {
                    var indexOfCommandArgs = part.IndexOf('<');
                    if (indexOfCommandArgs != -1)
                        // cmd <arg>
                        return part[..(indexOfCommandArgs - 1)];
                    return part;
                }
            ).ToArray();
    }
}

public static class DumpHeap
{
    // TODO: regex, single-line parsing
    public static CommandOutput Parse(string command, string[] output)
    {
        var mainRangeStart =
            output.IndexAfter(x => x.Contains("Address ") && x.Contains(" MT ") && x.Contains(" Size"));

        var statisticsStart = output.IndexAfter("Statistics:");
        // skip statistics header if statistics are present
        if (statisticsStart > 0) statisticsStart++;

        var statisticsEnd = output.IndexBefore(x => x.Contains("Total ") && x.Contains(" objects"));
        var foundSectionEnd = output.IndexBefore(x => x.Contains("Found ") && x.Contains(" objects"));

        // there are 4 lines between main section data and statistics data
        var mainIndexesEnd =
            statisticsStart == -1
                ? foundSectionEnd
                : statisticsStart - 4;

        var (mainRange, mainHeaderRanges) =
            mainRangeStart == -1
                ? default
                : (new Range(mainRangeStart, mainIndexesEnd + 1),
                    GetDumpHeapHeaderRanges(output[mainRangeStart - 1]));

        var (statisticsRange, statisticsHeaderRanges) =
            statisticsStart == -1
                ? default
                : (new Range(statisticsStart, statisticsEnd + 1),
                    GetDumpHeapStatisticsHeaderRanges(output[statisticsStart - 1]));

        return new(command, output.MapRange(
            x => new(x),
            new RangeMapper<string, OutputLine>(mainRange, x => new DumpHeapOutputLine(x, mainHeaderRanges)),
            new RangeMapper<string, OutputLine>(statisticsRange,
                x => new DumpHeapStatisticsOutputLine(x, statisticsHeaderRanges))
        ));
    }

    // TODO: regex, single-line parsing
    public static DumpHeapRanges GetDumpHeapHeaderRanges(string header)
    {
        // TODO: regex
        var address = header.FindColumnRange("Address");
        var mt = header.FindColumnRange("MT", address);
        var size = header.FindColumnRange("Size", mt);
        return new(address, mt, size);
    }

    // TODO: regex, single-line parsing
    public static DumpHeapStatisticsRanges GetDumpHeapStatisticsHeaderRanges(string header)
    {
        // TODO: regex
        var mt = header.FindColumnRange("MT");
        var count = header.FindColumnRange("Count", mt);
        var totalSize = header.FindColumnRange("TotalSize", count);
        var className = header.FindColumnRange("Class Name", totalSize, true);
        return new(mt, count, totalSize, className);
    }
}

public static class SetThread
{
    public static readonly Regex OsIdParser = new(RegexPatterns.OsId);

    public static ReadOnlyMemory<char> GetOsIDFromSetThreadLine(string lineWithOsId)
    {
        var group = OsIdParser.Match(lineWithOsId).Groups[1];
        var osIdRange = group.GetRange();
        return lineWithOsId.AsMemory()[osIdRange];
    }

    // TODO: regex, single-line parsing
    public static CommandOutput Parse(string command, string[] output)
    {
        return new(command, output.Map(x =>
        {
            if (OsIdParser.IsMatch(x))
                return new SetThreadOutputLine(x);

            return new OutputLine(x);
        }));
    }
}

public static class ClrThreads
{
    public static readonly Regex ClrThreadsRegex = new(RegexPatterns.ClrThreads, RegexOptions.Compiled);

    public static CommandOutput Parse(string command, string[] output)
    {
        return new(command, output.Map(x =>
        {
            if (x.AsSpan().TrimStart().Length > 100 && GetClrThreadsRanges(x) is {} ranges)
            {

                return new ClrThreadsOutputLine(x, ranges);
            }

            return new OutputLine(x);
        }));
    }

    public static ClrThreadsRanges? GetClrThreadsRanges(string line)
    {
        if (ClrThreadsRegex.Match(line) is
            {
                Success: true,
            } match)
        {
            var ranges = new Range[11];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0], ranges[1], ranges[2], ranges[3], ranges[4], ranges[5], ranges[6], ranges[7], ranges[8], ranges[9], ranges[10]);
        }

        return default;
    }
}
