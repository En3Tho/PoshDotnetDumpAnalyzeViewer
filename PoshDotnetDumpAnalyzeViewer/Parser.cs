using System.Text.RegularExpressions;

namespace PoshDotnetDumpAnalyzeViewer;

public static class Parser
{
    public static class Help
    {
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
            // usually there is 42 lines for commands column, next is description column
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

        public static DumpHeapRanges GetDumpHeapHeaderRanges(string header)
        {
            // TO regex ?

            var address = header.FindColumnRange("Address");
            var mt = header.FindColumnRange("MT", address);
            var size = header.FindColumnRange("Size", mt);
            return new(address, mt, size);
        }

        public static DumpHeapStatisticsRanges GetDumpHeapStatisticsHeaderRanges(string header)
        {
            // TO regex ?
            var mt = header.FindColumnRange("MT");
            var count = header.FindColumnRange("Count", mt);
            var totalSize = header.FindColumnRange("TotalSize", count);
            var className = header.FindColumnRange("Class Name", totalSize, true);
            return new(mt, count, totalSize, className);
        }
    }

    public static class SetThread
    {
        public static readonly Regex OsIdParser = new(@"[\*|\s]+\d+\s+(0x.+)\s*\(\d+\)");

        public static ReadOnlyMemory<char> GetOsIDFromSetThreadLine(string lineWithOsId)
        {
            var group = OsIdParser.Match(lineWithOsId).Groups[1];
            var osIdRange = group.GetRange();
            return lineWithOsId.AsMemory()[osIdRange];
        }

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
}