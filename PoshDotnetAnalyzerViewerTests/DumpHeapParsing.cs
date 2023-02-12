using PoshDotnetDumpAnalyzeViewer;
using Xunit;

namespace PoshDotnetAnalyzerViewerTests;

public class DumpHeapParsing
{
    [Fact]
    public void TestThatDumpHeapDefaultOutputIsParsedCorrectly()
    {
        var output = new[]
        {
            "> dumpheap",
            "         Address               MT     Size",
            "000002a724541000 000002a722993230       24 Free",
            "000002a724541018 000002a722993230       24 Free",
            "000002a724541030 000002a722993230       24 Free",
            "",
            "Statistics:",
            "              MT    Count    TotalSize Class Name",
            "00007fff4b774a70        1           24 System.IO.SyncTextReader",
            "00007fff4b773e68        1           24 System.Threading.Tasks.Task+<>c",
            "00007fff4b72e0c8        1           24 System.IO.Stream+NullStream",
            "Total 3 objects"
        };

        var lines = OutputParserExtensions.Parse(new DumpHeapOutputParser(), output);

        Assert.True(lines is
        [
            {},
            {},
            DumpHeapOutputLine { Address.Span: "000002a724541000", MethodTable.Span: "000002a722993230" },
            DumpHeapOutputLine { Address.Span: "000002a724541018", MethodTable.Span: "000002a722993230" },
            DumpHeapOutputLine { Address.Span: "000002a724541030", MethodTable.Span: "000002a722993230" },
            {},
            {},
            {},
            DumpHeapStatisticsOutputLine { MethodTable.Span: "00007fff4b774a70", TypeName.Span: "System.IO.SyncTextReader" },
            DumpHeapStatisticsOutputLine { MethodTable.Span: "00007fff4b773e68", TypeName.Span: "System.Threading.Tasks.Task+<>c" },
            DumpHeapStatisticsOutputLine { MethodTable.Span: "00007fff4b72e0c8", TypeName.Span: "System.IO.Stream+NullStream" },
            {}
        ]);
    }

    [Fact]
    public void TestThatDumpHeapStatOutputIsParsedCorrectly()
    {
        var output = new[]
        {
            "> dumpheap -stat",
            "Statistics:",
            "              MT    Count    TotalSize Class Name",
            "00007fff4b774a70        1           24 System.IO.SyncTextReader",
            "00007fff4b773e68        1           24 System.Threading.Tasks.Task+<>c",
            "00007fff4b72e0c8        1           24 System.IO.Stream+NullStream",
            "Total 3 objects"
        };

        var lines = OutputParserExtensions.Parse(new DumpHeapOutputParser(), output);

        Assert.True(lines is
        [
            {},
            {},
            {},
            DumpHeapStatisticsOutputLine { MethodTable.Span: "00007fff4b774a70", TypeName.Span: "System.IO.SyncTextReader" },
            DumpHeapStatisticsOutputLine { MethodTable.Span: "00007fff4b773e68", TypeName.Span: "System.Threading.Tasks.Task+<>c" },
            DumpHeapStatisticsOutputLine { MethodTable.Span: "00007fff4b72e0c8", TypeName.Span: "System.IO.Stream+NullStream" },
            {}
        ]);
    }
}