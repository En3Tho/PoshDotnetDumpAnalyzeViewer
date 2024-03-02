using System.Text.RegularExpressions;
using PoshDotnetDumpAnalyzeViewer.Parsing;

namespace PoshDotnetDumpAnalyzeViewer.Utilities;

public static class CancellationTokenSourceExtensions
{
    public static async Task<T> AwaitAndCancel<T>(this CancellationTokenSource @this, Task<T> job)
    {
        try
        {
            return await job;
        }
        finally
        {
            @this.Cancel();
        }
    }
}

public static class GroupExtensions
{
    public static Range GetRange(this Group @this)
    {
        return
            @this.ValueSpan.Length == 0
                ? new Range()
                : new(@this.Index, @this.Index + @this.Length);
    }
}

public static class MatchExtensions
{
    public static void CopyGroupsRangesTo(this Match @this, Span<Range> ranges)
    {
        var i = 0;
        foreach (var group in @this.Groups.Cast<Group>().Skip(1).Take(ranges.Length))
        {
            var range = group.GetRange();
            ranges[i++] = range;
        }

        if (i != ranges.Length)
            throw new ArgumentException("Ranges length is different from match groups count");
    }
}

public static class SpanExtensions
{
    public static void Map<TIn, TOut>(this ReadOnlySpan<TIn> @this, Span<TOut> result, Func<TIn, TOut> mapper)
    {
        if (@this.Length != result.Length)
            throw new ArgumentException("Span's lengths are different");

        for (var i = 0; i < @this.Length; i++)
            result[i] = mapper(@this[i]);
    }

    public static void Map<TIn, TOut>(this Span<TIn> @this, Span<TOut> result, Func<TIn, TOut> mapper) =>
        Map((ReadOnlySpan<TIn>)@this, result, mapper);
}

public static class ArrayExtensions
{
    public static TOut[] Map<TIn, TOut>(this TIn[] @this, Func<TIn, TOut> mapper)
    {
        var result = new TOut[@this.Length];
        @this.AsSpan().Map(result.AsSpan(), mapper);
        return result;
    }
}

public static class OutputParserExtensions
{
    public static OutputLine[] ParseAll<T>(string[] lines, string command) where T : IOutputParser
    {
        var parser = (string line) => T.Parse(line, command);
        return lines.Map(parser);
    }
}