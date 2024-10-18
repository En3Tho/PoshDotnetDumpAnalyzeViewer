namespace PoshDotnetDumpAnalyzeViewer.Parsing;

public static class ParserUtilities
{
    public static int GetIntOsThreadId(this IOsThreadId osThreadId)
    {
        return OsThreadIdReader.Read(osThreadId.OsThreadId);
    }
}

public static class OsThreadIdReader
{
    public static int Read(ReadOnlySpan<char> span)
    {
        span = span.Trim();
        if (span.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            span = span[2..];
        }
        return Convert.ToInt32(span.ToString(), 16);
    }
}