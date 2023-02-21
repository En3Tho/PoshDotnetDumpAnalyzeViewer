using System.Text.RegularExpressions;

namespace PoshDotnetDumpAnalyzeViewer;

static class RegexPatterns
{
    private const string D = @"-?\d+";
    private const string H = @"(?:0[xX])?[0-9a-fA-F]+";
    private const string A = @"[0-9a-fA-F]{16}"; // address
    private const string S = @"\S+";
    private const string C = @".+";
    private const string azD = @"[a-z\d]+";
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
        $@"{WSo}({azD}(?:,{WS}{azD})*)(?:<{C}>)?{C}";

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

    public static class DumpObject
    {
        public const string CmdWithAddress =
            $"do{WSo}{Ag}";

        public const string TypeName =
            $"Name:{WSo}{Sg}";

        public const string MethodTable =
            $"MethodTable:{WSo}{Ag}";

        public const string EEClass =
            $"EEClass:{WSo}{Ag}";
    }
}

public partial class HelpParser : IOutputParser
{
    [GeneratedRegex(RegexPatterns.Help)]
    public static partial Regex Regex();

    public static OutputLine Parse(string line)
    {
        if (Regex().IsMatch(line))
            return new HelpOutputLine(line);

        return new(line);
    }


    public static string[] GetCommandsFromLine(string line)
    {
        return Regex().Match(line).Groups[1].Value.Split(",", StringSplitOptions.TrimEntries);
    }
}

public partial class DumpHeapParser : IOutputParser
{
    [GeneratedRegex(RegexPatterns.DumpHeap)]
    public static partial Regex DumpHeapRegex();

    [GeneratedRegex(RegexPatterns.DumpHeapStatistics)]
    public static partial Regex DumpHeapStatisticsRegex();

    public static OutputLine Parse(string line)
    {
        if (GetDumpHeapHeaderRanges(line) is {} dumpHeapRanges)
            return new DumpHeapOutputLine(line, dumpHeapRanges);

        if (GetDumpHeapStatisticsHeaderRanges(line) is {} dumpHeapStatisticsRanges)
            return new DumpHeapStatisticsOutputLine(line, dumpHeapStatisticsRanges);

        return new(line);
    }

    public static DumpHeapRanges? GetDumpHeapHeaderRanges(string line)
    {
        if (DumpHeapRegex().Match(line) is { Success: true } match)
        {
            var ranges = new Range[3];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0], ranges[1], ranges[2]);
        }

        return default;
    }

    public static DumpHeapStatisticsRanges? GetDumpHeapStatisticsHeaderRanges(string line)
    {
        if (DumpHeapStatisticsRegex().Match(line) is { Success: true } match)
        {
            var ranges = new Range[4];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0], ranges[1], ranges[2], ranges[3]);
        }

        return default;
    }
}

public partial class SetThreadParser : IOutputParser
{
    [GeneratedRegex(RegexPatterns.OsId)]
    public static partial Regex Regex();

    public static OutputLine Parse(string line)
    {
        if (GetRanges(line) is {} ranges)
            return new SetThreadOutputLine(line, ranges);

        return new(line);
    }

    public static SetThreadRanges? GetRanges(string line)
    {
        if (Regex().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }
}

public partial class ClrThreadsParser : IOutputParser
{
    [GeneratedRegex(RegexPatterns.ClrThreads)]
    public static partial Regex Regex();

    public static OutputLine Parse(string line)
    {
        if (GetRanges(line) is {} ranges)
            return new ClrThreadsOutputLine(line, ranges);

        return new(line);
    }

    public static ClrThreadsRanges? GetRanges(string line)
    {
        if (Regex().Match(line) is { Success: true } match)
        {
            var ranges = new Range[11];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0], ranges[1], ranges[2], ranges[3], ranges[4], ranges[5], ranges[6], ranges[7], ranges[8], ranges[9], ranges[10]);
        }

        return default;
    }
}

public partial class SyncBlockParser : IOutputParser
{
    [GeneratedRegex(RegexPatterns.SyncBlock)]
    public static partial Regex Regex();

    public static OutputLine Parse(string line)
    {
        if (GetRanges(line) is {} ranges)
            return new SyncBlockOutputLine(line, ranges);

        return new(line);
    }

    public static SyncBlockRanges? GetRanges(string line)
    {
        if (Regex().Match(line) is { Success: true } match)
        {
            var ranges = new Range[9];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0], ranges[1], ranges[2], ranges[3], ranges[4], ranges[5], ranges[6], ranges[7], ranges[8]);
        }

        return default;
    }
}

public partial class DumpObjectParser : IOutputParser
{
    [GeneratedRegex(RegexPatterns.DumpObject.CmdWithAddress)]
    public static partial Regex CmdWithAddress();

    [GeneratedRegex(RegexPatterns.DumpObject.MethodTable)]
    public static partial Regex MethodTable();

    [GeneratedRegex(RegexPatterns.DumpObject.TypeName)]
    public static partial Regex TypeName();

    [GeneratedRegex(RegexPatterns.DumpObject.EEClass)]
    public static partial Regex EEClass();

    public static OutputLine Parse(string line)
    {
        if (GetObjectAddressRanges(line) is {} objectAddressRanges)
            return new ObjectAddressOutputLine(line, objectAddressRanges);

        if (GetMethodTableRanges(line) is {} methodTableRanges)
            return new MethodTableOutputLine(line, methodTableRanges);

        if (GetTypeNameRanges(line) is {} typeNameRanges)
            return new TypeNameOutputLine(line, typeNameRanges);

        if (GetEEClassRanges(line) is {} eeClassRanges)
            return new EEClassOutputLine(line, eeClassRanges);

        return new(line);
    }

    public static ObjectAddressRanges? GetObjectAddressRanges(string line)
    {
        if (CmdWithAddress().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }

    public static MethodTableRanges? GetMethodTableRanges(string line)
    {
        if (MethodTable().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }

    public static TypeNameRanges? GetTypeNameRanges(string line)
    {
        if (TypeName().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }

    public static EEClassRanges? GetEEClassRanges(string line)
    {
        if (EEClass().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }
}