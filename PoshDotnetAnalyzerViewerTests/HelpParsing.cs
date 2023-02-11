using PoshDotnetDumpAnalyzeViewer;
using Xunit;

namespace PoshDotnetAnalyzerViewerTests;

public class HelpParsing
{
    [Fact]
    public void TestThatDumpHeapRangesAreParsedCorrectly()
    {
        var commands = Help.GetCommandsFromLine("  dumpheap <arguments>                       Displays info about the garbage-collected heap and collection statistics about objects.");
        Assert.Equal("dumpheap", commands[0]);

        commands = Help.GetCommandsFromLine("  clrmodules                                 Lists the managed modules in the process.");
        Assert.Equal("clrmodules", commands[0]);

        commands = Help.GetCommandsFromLine("  lm, modules                                Displays the native modules in the process.");
        Assert.Equal("lm", commands[0]);
        Assert.Equal("modules", commands[1]);

        commands = Help.GetCommandsFromLine(" dso, dumpstackobjects <arguments>          Displays all managed objects found within the bounds of the current stack.");
        Assert.Equal("dso", commands[0]);
        Assert.Equal("dumpstackobjects", commands[1]);
    }

}