using PoshDotnetDumpAnalyzeViewer;
using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using Xunit;

namespace PoshDotnetAnalyzerViewerTests;

public class DumpClassParsing
{
    [Fact]
    public void TestThatDumpClassIsParsedCorrectly()
    {
        var output = new[]
        {
            "> dumpclass 00007F003E9F66D8",
            "Class Name:      System.Object[]",
            "mdToken:         0000000002000000",
            "File:            /usr/share/dotnet/shared/Microsoft.NETCore.App/5.0.17/System.Private.CoreLib.dll",
            "Parent Class:    00007f003ea1d908",
            "Module:          00007f003dfe4020",
            "Method Table:    00007f003e9f6760",
            "Vtable Slots:    18",
            "Total Method Slots:  1c",
            "Class Attributes:    2101",
            "NumInstanceFields:   0",
            "NumStaticFields:     0"
        };

        var lines = OutputParserExtensions.ParseAll<DumpClassParser>(output, Commands.DumpClass);

        Assert.True(lines is
        [
            {},
            TypeNameOutputLine { TypeName: "System.Object[]" },
            {},
            {},
            EEClassAddressOutputLine { EEClassAddress: "00007f003ea1d908" },
            ModuleAddressOutputLine { ModuleAddress: "00007f003dfe4020" },
            MethodTableOutputLine { MethodTable: "00007f003e9f6760" },
            {},
            {},
            {},
            {},
            {},
        ]);
    }
}