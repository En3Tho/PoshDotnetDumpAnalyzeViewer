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
            var mainIndexesStart =
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

            var (mainRange, mainIndexes) =
                mainIndexesStart == -1
                    ? default
                    : (new Range(mainIndexesStart, mainIndexesEnd + 1),
                        GetDumpHeapHeaderIndices(output[mainIndexesStart - 1]));


            var (statisticsRange, statisticsIndexes) =
                statisticsStart == -1
                    ? default
                    : (new Range(statisticsStart, statisticsEnd + 1),
                        GetDumpHeapStatisticsHeaderIndexes(output[statisticsStart - 1]));

            return new(command, output.MapRange(
                x => new(x),
                new RangeMapper<string, OutputLine>(mainRange, x => new DumpHeapOutputLine(x, mainIndexes)),
                new RangeMapper<string, OutputLine>(statisticsRange,
                    x => new DumpHeapStatisticsOutputLine(x, statisticsIndexes))
            ));
        }

        public static DumpHeapIndexes GetDumpHeapHeaderIndices(string header)
        {
            var address = header.FindColumnRange("Address");
            var mt = header.FindColumnRange("MT", address);
            var size = header.FindColumnRange("Size", mt);
            return new(address, mt, size);
        }

        public static DumpHeapStatisticsIndexes GetDumpHeapStatisticsHeaderIndexes(string header)
        {
            var mt = header.FindColumnRange("MT");
            var count = header.FindColumnRange("Count", mt);
            var totalSize = header.FindColumnRange("TotalSize", count);
            var className = header.FindColumnRange("Class Name", totalSize, true);
            return new(mt, count, totalSize, className);
        }
    }

    public static class SetThread
    {
        public static readonly Regex IndexParser = new(@"[\*|\s]+\d+\s+(0x.+)\s*\(\d+\)");

        public static ReadOnlyMemory<char> GetOsIDFromSetThreadLine(string lineWithOsId)
        {
            var group = IndexParser.Match(lineWithOsId).Groups[1];
            // ToExtension
            var osIdRange = new Range(group.Index, group.Length + group.Index - 1);
            return lineWithOsId.AsMemory()[osIdRange];
        }

        public static CommandOutput Parse(string command, string[] output)
        {
            var threadsIndexesStart = output.IndexAfter(x => x.StartsWith(">"));
            return new(command, output.Map(x =>
            {
                if (IndexParser.IsMatch(x))
                    return new SetThreadOutputLine(x);

                return new OutputLine(x);
            }));
        }
    }
}