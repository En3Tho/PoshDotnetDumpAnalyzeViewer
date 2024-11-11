using PoshDotnetDumpAnalyzeViewer;
using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using Xunit;

namespace PoshDotnetAnalyzerViewerTests;

public class DumpArrayParsing
{
    [Fact]
    public void TestThatDumpModuleIsParsedCorrectly()
    {
        var output = new[]
        {
            "> dumparray 000001b30a02c1d8",
            "Name:        System.Collections.Generic.Dictionary`2+Entry[[AccessSoftek.DataSource.Business.Interface.Types.User.DSMemberNumber, ASDS.Core.Interface],[AccessSoftek.DataSource.Business.Interface.Data.Consumer.DSRestrictedAccountInfo, ASDS.Business.Interface]][]",
            "MethodTable: 00007ffd571814f8",
            "EEClass:     00007ffd57181460",
            "Size:        96(0x60) bytes",
            "Array:       Rank 1, Number of elements 3, Type VALUETYPE",
            "Element Methodtable: 00007ffd571813c8",
            "[0] 000001b30a02c1e8",
            "[1] 000001b30a02c200",
            "[2] 000001b30a02c218"
        };

        var lines = OutputParserExtensions.ParseAll<DumpArrayParser>(output, Commands.DumpArray);

        Assert.True(lines is [
            ObjectAddressOutputLine { Address: "000001b30a02c1d8" },
            TypeNameOutputLine { TypeName: "System.Collections.Generic.Dictionary`2+Entry[[AccessSoftek.DataSource.Business.Interface.Types.User.DSMemberNumber, ASDS.Core.Interface],[AccessSoftek.DataSource.Business.Interface.Data.Consumer.DSRestrictedAccountInfo, ASDS.Business.Interface]][]" },
            MethodTableOutputLine { MethodTable: "00007ffd571814f8" },
            EEClassAddressOutputLine { EEClassAddress: "00007ffd57181460" },
            {},
            {},
            MethodTableOutputLine { MethodTable: "00007ffd571813c8" },
            ObjectAddressOutputLine { Address: "000001b30a02c1e8" },
            ObjectAddressOutputLine { Address: "000001b30a02c200" },
            ObjectAddressOutputLine { Address: "000001b30a02c218" }
        ]);
    }
}