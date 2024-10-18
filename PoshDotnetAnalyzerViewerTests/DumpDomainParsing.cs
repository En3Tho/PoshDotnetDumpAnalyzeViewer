using PoshDotnetDumpAnalyzeViewer;
using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using Xunit;

namespace PoshDotnetAnalyzerViewerTests;

public class DumpDomainParsing
{
    [Fact]
    public void TestThatDumpDomainIsParsedCorrectly()
    {
        var output = new[]
        {
            "> dumpdomain 000055a42e3ce3e0",
            "--------------------------------------",
            "Domain 1:           000055a42e3ce3e0",
            "LowFrequencyHeap:   00007F00B825E4A0",
            "HighFrequencyHeap:  00007F00B825E528",
            "StubHeap:           00007F00B825E5B0",
            "Stage:              OPEN",
            "Name:               clrhost",
            "Assembly:           000055a42e4d9030 [/usr/share/dotnet/shared/Microsoft.NETCore.App/5.0.17/System.Private.CoreLib.dll]",
            "ClassLoader:        000055A42E4D9090",
            "  Module",
            "  00007f003dfe4020    /usr/share/dotnet/shared/Microsoft.NETCore.App/5.0.17/System.Private.CoreLib.dll",
            "",
            "Assembly:           000055a42e3e3150 [/usr/share/dotnet/shared/Microsoft.NETCore.App/5.0.17/System.Runtime.dll]",
            "ClassLoader:        000055A42E568D00",
            "  Module",
            "  00007f003ead44a8    /usr/share/dotnet/shared/Microsoft.NETCore.App/5.0.17/System.Runtime.dll",
        };

        var lines = OutputParserExtensions.ParseAll<DumpDomainParser>(output, Commands.DumpDomain);

        Assert.True(lines is
        [
            {},
            {},
            {},
            {},
            {},
            {},
            {},
            {},
            AssemblyAddressOutputLine { AssemblyAddress: "000055a42e4d9030" },
            {},
            {},
            ModuleAddressOutputLine { ModuleAddress: "00007f003dfe4020" },
            {},
            AssemblyAddressOutputLine { AssemblyAddress: "000055a42e3e3150" },
            {},
            {},
            ModuleAddressOutputLine { ModuleAddress: "00007f003ead44a8" },
        ]);
    }
}