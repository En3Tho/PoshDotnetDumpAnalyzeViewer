namespace PoshDotnetDumpAnalyzeViewer;

public static class OutputLinesExtensions
{
    public static int GetIntOsThreadId(this IOsThreadId osThreadId)
    {
        return Utilities.GetIntOsThreadId(osThreadId.OsThreadId.Span);
    }
}

public static class Utilities
{
    public static int GetIntOsThreadId(ReadOnlySpan<char> span)
    {
        span = span.Trim();
        if (span.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            span = span[2..];
        }
        return Convert.ToInt32(span.ToString(), 16);
    }
}