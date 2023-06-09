using Xunit;

namespace PoshDotnetAnalyzerViewerTests;

public class CommandsTests
{
    [Fact]
    public static void TestThatCommandNormalizartionWorks()
    {
        Assert.Equal("dumpheap -stat", PoshDotnetDumpAnalyzeViewer.Commands.NormalizeCommand("DUMPHEAP -stat"));
        Assert.Equal("sos dumpheap -stat", PoshDotnetDumpAnalyzeViewer.Commands.NormalizeCommand("sOs DUMPHEAP -stat"));
        Assert.Equal("dumpheap -type System.String -stat", PoshDotnetDumpAnalyzeViewer.Commands.NormalizeCommand("dumPHeap -type System.String -stat"));
    }
}