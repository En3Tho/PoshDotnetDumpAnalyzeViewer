using PoshDotnetDumpAnalyzeViewer;
using Xunit;

namespace PoshDotnetAnalyzerViewerTests;

public class DumpHeapParsing
{
    [Fact]
    public void TestThatDumpHeapIndicesAreParsedCorrectly()
    {
        var indices = Parser.DumpHeap.GetDumpHeapHeaderIndices("         Address               MT     Size");
        var line = new DumpHeapOutputLine("000002a724541000 000002a722993230       24 Free", indices);
        Assert.Equal("000002a724541000", line.Address.ToString());
        Assert.Equal("000002a722993230", line.MethodTable.ToString());
    }

    [Fact]
    public void TestThatDumpHeapStatisticsIndicesAreParsedCorrectly()
    {
        var indices = Parser.DumpHeap.GetDumpHeapStatisticsHeaderIndexes("              MT    Count    TotalSize Class Name");
        var line = new DumpHeapStatisticsOutputLine("00007fff4b774a70        1           24 System.IO.SyncTextReader", indices);
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

        var idx = 0;
        Assert.IsType<OutputLine>(lines[idx++]);
        Assert.IsType<OutputLine>(lines[idx++]);
        Assert.IsType<DumpHeapOutputLine>(lines[idx++]);
        Assert.IsType<DumpHeapOutputLine>(lines[idx++]);
        Assert.IsType<DumpHeapOutputLine>(lines[idx++]);
        Assert.IsType<OutputLine>(lines[idx++]);
        Assert.IsType<OutputLine>(lines[idx++]);
        Assert.IsType<OutputLine>(lines[idx++]);
        Assert.IsType<DumpHeapStatisticsOutputLine>(lines[idx++]);
        Assert.IsType<DumpHeapStatisticsOutputLine>(lines[idx++]);
        Assert.IsType<DumpHeapStatisticsOutputLine>(lines[idx++]);
        Assert.IsType<OutputLine>(lines[idx++]);
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

        var idx = 0;
        Assert.IsType<OutputLine>(lines[idx++]);
        Assert.IsType<OutputLine>(lines[idx++]);
        Assert.IsType<OutputLine>(lines[idx++]);
        Assert.IsType<DumpHeapStatisticsOutputLine>(lines[idx++]);
        Assert.IsType<DumpHeapStatisticsOutputLine>(lines[idx++]);
        Assert.IsType<DumpHeapStatisticsOutputLine>(lines[idx++]);
        Assert.IsType<OutputLine>(lines[idx++]);
    }
}