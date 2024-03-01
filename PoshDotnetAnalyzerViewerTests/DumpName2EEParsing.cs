using PoshDotnetDumpAnalyzeViewer;
using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using Xunit;

namespace PoshDotnetAnalyzerViewerTests;

public class DumpName2EEParsing
{
    [Fact]
    public void TestThatDumpModuleIsParsedCorrectly()
    {
        var output = new[]
        {
            "> name2ee *!System.Exception",
            "Module:      00007f003dfe4020",
            "Assembly:    System.Private.CoreLib.dll",
            "Token:       000000000200004F",
            "MethodTable: 00007f003eab93e0",
            "EEClass:     00007f003eaa3530",
            "Name:        System.Exception",
            "--------------------------------------",
            "Module:      00007f003ead44a8",
            "Assembly:    System.Runtime.dll",
            "--------------------------------------",
            "Module:      00007f003eb46ed8",
            "Assembly:    Microsoft.Extensions.Hosting.Abstractions.dll",
        };

        var lines = OutputParserExtensions.ParseAll<Name2EEParser>(output, Commands.Name2EE);

        Assert.True(lines is
        [
            {},
            ModuleAddressOutputLine { ModuleAddress.Span: "00007f003dfe4020" },
            not AssemblyAddressOutputLine,
            {},
            MethodTableOutputLine { MethodTable.Span: "00007f003eab93e0" },
            EEClassAddressOutputLine { EEClassAddress.Span: "00007f003eaa3530" },
            TypeNameOutputLine { TypeName.Span: "System.Exception" },
            {},
            ModuleAddressOutputLine { ModuleAddress.Span: "00007f003ead44a8" },
            not AssemblyAddressOutputLine,
            {},
            ModuleAddressOutputLine { ModuleAddress.Span: "00007f003eb46ed8" },
            not AssemblyAddressOutputLine,
        ]);
    }
}