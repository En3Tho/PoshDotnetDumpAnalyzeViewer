namespace PoshDotnetDumpAnalyzeViewer;

public static class Parser
{
    public static class Help
    {
        public static string[] GetCommandsFromLine(string line)
        {
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
}