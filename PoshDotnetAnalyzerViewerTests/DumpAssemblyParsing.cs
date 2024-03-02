using PoshDotnetDumpAnalyzeViewer;
using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using Xunit;

namespace PoshDotnetAnalyzerViewerTests;

public class DumpAssemblyParsing
{
    [Fact]
    public void TestThatDumpModuleIsParsedCorrectly()
    {
        var output = new[]
        {
            "> dumpassembly 000055a42e4d9030",
            "Parent Domain:      000055a42e3ce3e0",
            "Name:               /usr/share/dotnet/shared/Microsoft.NETCore.App/5.0.17/System.Private.CoreLib.dll",
            "ClassLoader:        000055A42E4D9090",
            "Module",
            "00007f003dfe4020    /usr/share/dotnet/shared/Microsoft.NETCore.App/5.0.17/System.Private.CoreLib.dll",
        };

        var lines = OutputParserExtensions.ParseAll<DumpAssemblyParser>(output, Commands.DumpAssembly);

        Assert.True(lines is
        [
            {},
            DomainAddressOutputLine { DomainAddress: "000055a42e3ce3e0" },
            {},
            {},
            {},
            ModuleAddressOutputLine { ModuleAddress: "00007f003dfe4020" },
        ]);
    }
}