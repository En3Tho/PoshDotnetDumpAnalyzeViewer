using System.Text.RegularExpressions;

namespace PoshDotnetDumpAnalyzeViewer;

static class RegexPatterns
{
    private const string D = @"-?[0-9]+";
    private const string H = @"(?:0[xX])?[0-9a-fA-F]+";
    private const string A = @"[0-9a-fA-F]{16}"; // address
    private const string S = @"[^\s].*[^\s]";
    private const string C = @".+";
    private const string az = @"[a-z]+";
    private const string Co = @".*";
    private const string WS = @"\s+";
    private const string WSo = @"\s*";
    private const string Dg = $"({D})";
    private const string Hg = $"({H})";
    private const string Ag = $"({A})";
    private const string Sg = $"({S})";
    private const string Sgo = $"({S})*";

    // cmd (..,cmd)? <arg>? Description
    // S

    public const string Help =
        $@"{WSo}({az}(?:,{WS}{az})*)(?:<{C}>)?{C}";

    // ...   OsId ...
    // C WS  Hg    WS C
    public const string OsId =
        $"{C}{WS}{Hg}{WS}{C}";

    //                                                                              Lock
    // DBG           ID  OSID  ThreadOBJ  State  GC Mode  GC Alloc Context  Domain  Count  Apt   Exception
    // (D | "XXXX")g Dg  Hg    Ag         Hg     Sg       (A & ":" & A)g    Ag      Dg     Sg    Sg?
    public const string ClrThreads =
        $"{WSo}((?:XXXX)|{D}){WS}{Dg}{WS}{Hg}{WS}{Ag}{WS}{Hg}{WS}{Sg}{WS}({A}:{A}){WS}{Ag}{WS}{Dg}{WS}{Sg}{WSo}{Sgo}{WSo}";

    // Address  MT  Size ..rest
    // Ag       Ag  Dg
    public const string DumpHeap =
        $"{Ag}{WS}{Ag}{WS}{Dg}{C}";

    // MT  Count  TotalSize  Class Name
    // Ag  Dg     Dg         Sg
    public const string DumpHeapStatistics =
        $"{Ag}{WS}{Dg}{WS}{Dg}{WS}{Sg}{WSo}";

    // Index  SyncBlock  MonitorHeld  Recursion  Owning Thread Info  SyncBlock Owner
    // Dg     Ag         Dg           Dg         Ag Hg Dg            Ag Sg
    public const string SyncBlock =
        $"{WSo}{Dg}{WS}{Ag}{WS}{Dg}{WS}{Dg}{WS}{Ag}{WS}{Hg}{WS}{Dg}{WS}{Ag}{WS}{Sg}{WSo}";
}

public static class Help
{
    public static readonly Regex Regex = new(RegexPatterns.Help);

    // TODO: single-line parsing
    public static OutputLine Parse(string line)
    {
        if (Regex.IsMatch(line))
            return new HelpOutputLine(line);

        return new(line);
    }

    public static CommandOutput Parse(string command, string[] output)
    {
        return new(command, output.Map(Parse));
    }

    public static string[] GetCommandsFromLine(string line)
    {
        return Regex.Match(line).Groups[1].Value.Split(",", StringSplitOptions.TrimEntries);
    }
}

public static class DumpHeap
{
    public static readonly Regex DumpHeapRegex = new(RegexPatterns.DumpHeap);
    public static readonly Regex DumpHeapStatisticsRegex = new(RegexPatterns.DumpHeapStatistics);

    // TODO: regex, single-line parsing

    public static OutputLine Parse(string line)
    {
        if (GetDumpHeapHeaderRanges(line) is {} dumpHeapRanges)
            return new DumpHeapOutputLine(line, dumpHeapRanges);

        if (GetDumpHeapStatisticsHeaderRanges(line) is {} dumpHeapStatisticsRanges)
            return new DumpHeapStatisticsOutputLine(line, dumpHeapStatisticsRanges);

        return new(line);
    }

    public static CommandOutput Parse(string command, string[] output)
    {
        return new(command, output.Map(Parse));
    }

    public static DumpHeapRanges? GetDumpHeapHeaderRanges(string line)
    {
        if (DumpHeapRegex.Match(line) is { Success: true } match)
        {
            var ranges = new Range[3];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0], ranges[1], ranges[2]);
        }

        return default;
    }

    public static DumpHeapStatisticsRanges? GetDumpHeapStatisticsHeaderRanges(string line)
    {
        if (DumpHeapStatisticsRegex.Match(line) is { Success: true } match)
        {
            var ranges = new Range[4];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0], ranges[1], ranges[2], ranges[3]);
        }

        return default;
    }
}

public static class SetThread
{
    public static readonly Regex Regex = new(RegexPatterns.OsId);

    // TODO: single-line parsing

    public static OutputLine Parse(string line)
    {
        if (GetRanges(line) is {} ranges)
            return new SetThreadOutputLine(line, ranges);

        return new(line);
    }

    public static CommandOutput Parse(string command, string[] output)
    {
        return new(command, output.Map(Parse));
    }

    public static SetThreadRanges? GetRanges(string line)
    {
        if (Regex.Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }
}

public static class ClrThreads
{
    public static readonly Regex Regex = new(RegexPatterns.ClrThreads, RegexOptions.Compiled);

    public static OutputLine Parse(string line)
    {
        if (GetRanges(line) is {} ranges)
            return new ClrThreadsOutputLine(line, ranges);

        return new(line);
    }

    public static CommandOutput Parse(string command, string[] output)
    {
        return new(command, output.Map(Parse));
    }

    public static ClrThreadsRanges? GetRanges(string line)
    {
        if (Regex.Match(line) is { Success: true } match)
        {
            var ranges = new Range[11];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0], ranges[1], ranges[2], ranges[3], ranges[4], ranges[5], ranges[6], ranges[7], ranges[8], ranges[9], ranges[10]);
        }

        return default;
    }
}

public static class SyncBlock
{
    public static readonly Regex Regex = new(RegexPatterns.SyncBlock, RegexOptions.Compiled);

    public static OutputLine Parse(string line)
    {
        if (GetRanges(line) is {} ranges)
            return new SyncBlockOutputLine(line, ranges);

        return new(line);
    }

    public static CommandOutput Parse(string command, string[] output)
    {
        return new(command, output.Map(Parse));
    }

    public static SyncBlockRanges? GetRanges(string line)
    {
        if (Regex.Match(line) is { Success: true } match)
        {
            var ranges = new Range[9];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0], ranges[1], ranges[2], ranges[3], ranges[4], ranges[5], ranges[6], ranges[7], ranges[8]);
        }

        return default;
    }
}