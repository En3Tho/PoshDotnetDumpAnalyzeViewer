using PoshDotnetDumpAnalyzeViewer;
using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Utilities;
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

        var lines = OutputParserExtensions.ParseAll<DumpHeapParser>(output, Commands.DumpHeap);

        Assert.True(lines is
        [
            not (DumpHeapOutputLine or DumpHeapStatisticsOutputLine),
            not (DumpHeapOutputLine or DumpHeapStatisticsOutputLine),
            DumpHeapOutputLine { Address: "000002a724541000", MethodTable: "000002a722993230" },
            DumpHeapOutputLine { Address: "000002a724541018", MethodTable: "000002a722993230" },
            DumpHeapOutputLine { Address: "000002a724541030", MethodTable: "000002a722993230" },
            not (DumpHeapOutputLine or DumpHeapStatisticsOutputLine),
            not (DumpHeapOutputLine or DumpHeapStatisticsOutputLine),
            not (DumpHeapOutputLine or DumpHeapStatisticsOutputLine),
            DumpHeapStatisticsOutputLine { MethodTable: "00007fff4b774a70", TypeName: "System.IO.SyncTextReader" },
            DumpHeapStatisticsOutputLine { MethodTable: "00007fff4b773e68", TypeName: "System.Threading.Tasks.Task+<>c" },
            DumpHeapStatisticsOutputLine { MethodTable: "00007fff4b72e0c8", TypeName: "System.IO.Stream+NullStream" },
            not (DumpHeapOutputLine or DumpHeapStatisticsOutputLine)
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
            "7fff9b7859c0 3591478 341091786 System.String", // address is not 16 chars anymore
            "Total 3 objects"
        };

        var lines = OutputParserExtensions.ParseAll<DumpHeapParser>(output, Commands.DumpHeap);

        Assert.True(lines is
        [
            {},
            {},
            {},
            DumpHeapStatisticsOutputLine { MethodTable: "00007fff4b774a70", TypeName: "System.IO.SyncTextReader" },
            DumpHeapStatisticsOutputLine { MethodTable: "00007fff4b773e68", TypeName: "System.Threading.Tasks.Task+<>c" },
            DumpHeapStatisticsOutputLine { MethodTable: "00007fff4b72e0c8", TypeName: "System.IO.Stream+NullStream" },
            DumpHeapStatisticsOutputLine { MethodTable: "7fff9b7859c0", TypeName: "System.String" },
            {}
        ]);
    }
}