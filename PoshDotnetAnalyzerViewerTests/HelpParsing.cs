using PoshDotnetDumpAnalyzeViewer;
using Xunit;

namespace PoshDotnetAnalyzerViewerTests;

public class HelpParsing
{
    [Fact]
    public void TestThatDumpHeapRangesAreParsedCorrectly()
    {
        var commands = HelpParser.GetCommandsFromLine("  dumpheap <arguments>                       Displays info about the garbage-collected heap and collection statistics about objects.");
        Assert.True(commands is [ "dumpheap" ]);

        commands = HelpParser.GetCommandsFromLine("  clrmodules                                 Lists the managed modules in the process.");
        Assert.True(commands is [ "clrmodules" ]);

        commands = HelpParser.GetCommandsFromLine("  lm, modules                                Displays the native modules in the process.");
        Assert.True(commands is [ "lm", "modules" ]);

        commands = HelpParser.GetCommandsFromLine(" dso, dumpstackobjects <arguments>          Displays all managed objects found within the bounds of the current stack.");
        Assert.True(commands is [ "dso", "dumpstackobjects" ]);
    }

}