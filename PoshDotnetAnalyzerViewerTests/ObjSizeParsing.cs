using PoshDotnetDumpAnalyzeViewer;
using Xunit;

namespace PoshDotnetAnalyzerViewerTests;

public class ObjSizeParsing
{
    [Fact]
    public void TestThatObjSizeDefaultOutputIsParsedCorrectly()
    {
        var output = new[]
        {
            "> objsize 00007efc28036bc8",
            "Objects which 7efc28036bc8(System.Object[]) transitively keep alive:",
            "      Address           MT         Size",
            "7efc28036bc8 7f003e9f6760        65304",
            "7ef6284d32c8 7f0044ef10d0           24",
            "7ef6284d32e0 7f0044ef1340           64",
            "7ef6284d3398 7f0044e1eb18           64",
            "",
            "Statistics:",
            "            MT   Count  TotalSize Class Name",
            "7f0044bc3970  37699  9650944 System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Boolean>+AsyncStateMachineBox<MySqlConnector.Core.ServerSession+<TryResetConnectionAsync>d__77>",
            "7f003eab2498  88822 12843792 System.SByte[]",
            "7f003eab7a60 522111 40985402 System.String",
            "Total 6061980 objects, 470435152 bytes"
        };

        var lines = OutputParserExtensions.ParseAll<ObjSizeParser>(output, Commands.ObjSize);

        Assert.True(lines is
        [
            not (ObjSizeOutputLine or ObjSizeStatisticsOutputLine),
            not (ObjSizeOutputLine or ObjSizeStatisticsOutputLine),
            not (ObjSizeOutputLine or ObjSizeStatisticsOutputLine),
            ObjSizeOutputLine { Address.Span: "7efc28036bc8", MethodTable.Span: "7f003e9f6760" },
            ObjSizeOutputLine { Address.Span: "7ef6284d32c8", MethodTable.Span: "7f0044ef10d0" },
            ObjSizeOutputLine { Address.Span: "7ef6284d32e0", MethodTable.Span: "7f0044ef1340" },
            ObjSizeOutputLine { Address.Span: "7ef6284d3398", MethodTable.Span: "7f0044e1eb18" },
            not (ObjSizeOutputLine or ObjSizeStatisticsOutputLine),
            not (ObjSizeOutputLine or ObjSizeStatisticsOutputLine),
            not (ObjSizeOutputLine or ObjSizeStatisticsOutputLine),
            ObjSizeStatisticsOutputLine { MethodTable.Span: "7f0044bc3970", TypeName.Span: "System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Boolean>+AsyncStateMachineBox<MySqlConnector.Core.ServerSession+<TryResetConnectionAsync>d__77>" },
            ObjSizeStatisticsOutputLine { MethodTable.Span: "7f003eab2498", TypeName.Span: "System.SByte[]" },
            ObjSizeStatisticsOutputLine { MethodTable.Span: "7f003eab7a60", TypeName.Span: "System.String" },
            not (ObjSizeOutputLine or ObjSizeStatisticsOutputLine)
        ]);
    }
}