using PoshDotnetDumpAnalyzeViewer;
using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using Xunit;

namespace PoshDotnetAnalyzerViewerTests;

public class DumpModuleParsing
{
    [Fact]
    public void TestThatDumpModuleIsParsedCorrectly()
    {
        var output = new[]
        {
            "> dumpmodule 00007F003DFE4020",
            "Name: /usr/share/dotnet/shared/Microsoft.NETCore.App/5.0.17/System.Private.CoreLib.dll",
            "Attributes:              PEFile ",
            "TransientFlags:          00208811 ",
            "Assembly:                000055a42e4d9030",
            "BaseAddress:             00007F003E090000",
            "PEAssembly:              000055A42E3C8B40",
            "ModuleId:                00007F003E9F0020",
            "ModuleIndex:             0000000000000000",
            "LoaderHeap:              0000000000000000",
            "TypeDefToMethodTableMap: 00007F003E9A0020",
            "TypeRefToMethodTableMap: 00007F003E9A40B0",
            "MethodDefToDescMap:      00007F003E9A40B8",
            "FieldDefToDescMap:       00007F003E9D4830",
            "MemberRefToDescMap:      0000000000000000",
            "FileReferencesMap:       00007F003E9E7488",
            "AssemblyReferencesMap:   00007F003E9E7490",
            "MetaData start address:  00007F003E27B198 (1840180 bytes)"
        };

        var lines = OutputParserExtensions.ParseAll<DumpModuleParser>(output, Commands.DumpModule);

        Assert.True(lines is
        [
            {},
            {},
            {},
            {},
            AssemblyAddressOutputLine { AssemblyAddress.Span: "000055a42e4d9030" },
            {},
            {},
            {},
            {},
            {},
            {},
            {},
            {},
            {},
            {},
            {},
            {},
            {},
        ]);
    }
}