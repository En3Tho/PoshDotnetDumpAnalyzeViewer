using PoshDotnetDumpAnalyzeViewer;
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
            SyncBlockZeroOutputLine { SyncBlockIndex.Span: "1", SyncBlockAddress.Span: "000055A42E4605A8", SyncBlockOwnerAddress.Span: "00007ef6280010e0", SyncBlockOwnerTypeName.Span: "Interop+Advapi32+EtwEnableCallback" },
            SyncBlockZeroOutputLine { SyncBlockIndex.Span: "2", SyncBlockAddress.Span: "000055A42E4605F0", SyncBlockOwnerAddress.Span: "00007ef628001178", SyncBlockOwnerTypeName.Span: "Interop+Advapi32+EtwEnableCallback" },
            not (SyncBlockOutputLine or SyncBlockZeroOutputLine),
            SyncBlockZeroOutputLine { SyncBlockIndex.Span: "23", SyncBlockAddress.Span: "000055A42E460BD8", SyncBlockOwnerAddress.Span: "00007ef62803ad20", SyncBlockOwnerTypeName.Span: "System.Collections.Generic.Dictionary`2[[Microsoft.Extensions.DependencyInjection.ServiceLookup.ServiceCacheKey, Microsoft.Extensions.DependencyInjection],[System.Object, System.Private.CoreLib]]" },
            SyncBlockOutputLine { SyncBlockIndex.Span: "60238", SyncBlockAddress.Span: "00000000503B12B8", OsThreadId.Span: "16cc", SyncBlockOwnerAddress.Span: "00000001b6a30eb8", SyncBlockOwnerTypeName.Span: "System.Object" },
            not (SyncBlockOutputLine or SyncBlockZeroOutputLine),
            not (SyncBlockOutputLine or SyncBlockZeroOutputLine),
            not (SyncBlockOutputLine or SyncBlockZeroOutputLine),
            not (SyncBlockOutputLine or SyncBlockZeroOutputLine),
            not (SyncBlockOutputLine or SyncBlockZeroOutputLine),
            not (SyncBlockOutputLine or SyncBlockZeroOutputLine),
        ]);
    }
}