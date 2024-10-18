using PoshDotnetDumpAnalyzeViewer;
using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using Xunit;

namespace PoshDotnetAnalyzerViewerTests;

public class SyncBlockParsing
{
    [Fact]
    public void TestThaSyncBlockIsParsedCorrectly()
    {
        var output = new[]
        {
            "> syncblk -all",
            "Index         SyncBlock MonitorHeld Recursion Owning Thread Info          SyncBlock Owner", // this
            "    1 000055A42E4605A8            0         0 0000000000000000     none    00007ef6280010e0 Interop+Advapi32+EtwEnableCallback",
            "    2 000055A42E4605F0            0         0 0000000000000000     none    00007ef628001178 Interop+Advapi32+EtwEnableCallback",
            "    5 0000000000000000            0         0 0000000000000000     none           0 Free",
            "   23 000055A42E460BD8            0         0 0000000000000000     none    00007ef62803ad20 System.Collections.Generic.Dictionary`2[[Microsoft.Extensions.DependencyInjection.ServiceLookup.ServiceCacheKey, Microsoft.Extensions.DependencyInjection],[System.Object, System.Private.CoreLib]]",
            "60238 00000000503B12B8            3         1 0000000025F6D970 16cc  38   00000001b6a30eb8 System.Object",
            "-----------------------------",
            "Total           4",
            "CCW             0",
            "RCW             0",
            "ComClassFactory 0",
            "Free            34",
        };

        var lines = OutputParserExtensions.ParseAll<SyncBlockParser>(output, Commands.SyncBlock);

        Assert.True(lines is
        [
            not (SyncBlockOutputLine or SyncBlockZeroOutputLine),
            not (SyncBlockOutputLine or SyncBlockZeroOutputLine),
            SyncBlockZeroOutputLine { SyncBlockIndex: "1", SyncBlockAddress: "000055A42E4605A8", SyncBlockOwnerAddress: "00007ef6280010e0", SyncBlockOwnerTypeName: "Interop+Advapi32+EtwEnableCallback" },
            SyncBlockZeroOutputLine { SyncBlockIndex: "2", SyncBlockAddress: "000055A42E4605F0", SyncBlockOwnerAddress: "00007ef628001178", SyncBlockOwnerTypeName: "Interop+Advapi32+EtwEnableCallback" },
            not (SyncBlockOutputLine or SyncBlockZeroOutputLine),
            SyncBlockZeroOutputLine { SyncBlockIndex: "23", SyncBlockAddress: "000055A42E460BD8", SyncBlockOwnerAddress: "00007ef62803ad20", SyncBlockOwnerTypeName: "System.Collections.Generic.Dictionary`2[[Microsoft.Extensions.DependencyInjection.ServiceLookup.ServiceCacheKey, Microsoft.Extensions.DependencyInjection],[System.Object, System.Private.CoreLib]]" },
            SyncBlockOutputLine { SyncBlockIndex: "60238", SyncBlockAddress: "00000000503B12B8", OsThreadId: "16cc", SyncBlockOwnerAddress: "00000001b6a30eb8", SyncBlockOwnerTypeName: "System.Object" },
            not (SyncBlockOutputLine or SyncBlockZeroOutputLine),
            not (SyncBlockOutputLine or SyncBlockZeroOutputLine),
            not (SyncBlockOutputLine or SyncBlockZeroOutputLine),
            not (SyncBlockOutputLine or SyncBlockZeroOutputLine),
            not (SyncBlockOutputLine or SyncBlockZeroOutputLine),
            not (SyncBlockOutputLine or SyncBlockZeroOutputLine),
        ]);
    }
}