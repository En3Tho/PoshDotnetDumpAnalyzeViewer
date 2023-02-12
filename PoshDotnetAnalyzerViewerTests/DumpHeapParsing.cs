using PoshDotnetDumpAnalyzeViewer;
using Xunit;

namespace PoshDotnetAnalyzerViewerTests;

public class DumpHeapParsing
{
    [Fact]
    public void TestThatDumpHeapRangesAreParsedCorrectly()
    {
        var ranges = DumpHeap.GetDumpHeapHeaderRanges("         Address               MT     Size");
        var line = new DumpHeapOutputLine("000002a724541000 000002a722993230       24 Free", ranges);
        Assert.Equal("000002a724541000", line.Address.ToString());
        Assert.Equal("000002a722993230", line.MethodTable.ToString());
    }

    [Fact]
    public void TestThatDumpHeapStatisticsRangesAreParsedCorrectly()
    {
        var ranges = DumpHeap.GetDumpHeapStatisticsHeaderRanges("              MT    Count    TotalSize Class Name");
        var line = new DumpHeapStatisticsOutputLine("00007fff4b774a70        1           24 System.IO.SyncTextReader", ranges);
        Assert.Equal("System.IO.SyncTextReader", line.TypeName.ToString());
        Assert.Equal("00007fff4b774a70", line.MethodTable.ToString());
    }

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

        var parseResult = new DumpHeapOutputParser().Parse("", output);
        var lines = parseResult.Lines;

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

        var parseResult = new DumpHeapOutputParser().Parse("", output);
        var lines = parseResult.Lines;

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