namespace PoshDotnetDumpAnalyzeViewer;

public static class Parser
{
    public static class Help
    {
        public static string[] GetCommandsFromLine(string line)
        {
            return line.Split(",", StringSplitOptions.TrimEntries)
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
}