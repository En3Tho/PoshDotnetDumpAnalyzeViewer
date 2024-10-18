using PoshDotnetDumpAnalyzeViewer;
using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using Xunit;

namespace PoshDotnetAnalyzerViewerTests;

public class DumpMethodTableParsing
{
    [Fact]
    public void TestThatDumpMethodTableIsParsedCorrectly()
    {
        var output = new[]
        {
            "> dumpmt 00007f9f571c4e80",
            "EEClass:         00007F9F56DFCF48", // this
            "Module:          00007F9F55CB8278", // this
            "Name:            System.Collections.Immutable.SortedInt32KeyNode`1[[System.Collections.Immutable.ImmutableDictionary`2+HashBucket[[System.String, System.Private.CoreLib],[Microsoft.Build.Execution.ProjectMetadataInstance, Microsoft.Build]], System.Collections.Immutable]]",
            "mdToken:         0000000002000072", // this?
            "File:            /usr/share/dotnet/shared/Microsoft.NETCore.App/6.0.4/System.Collections.Immutable.dll",
            "BaseSize:        0x40",
            "ComponentSize:   0x0",
            "DynamicStatics:  true",
            "ContainsPointers true",
            "Slots in VTable: 33",
            "Number of IFaces in IFaceMap: 1"
        };

        var lines = OutputParserExtensions.ParseAll<DumpMethodTableParser>(output, Commands.DumpMethodTable);

        Assert.True(lines is
        [
            {},
            EEClassAddressOutputLine { EEClassAddress: "00007F9F56DFCF48" },
            ModuleAddressOutputLine { ModuleAddress: "00007F9F55CB8278" },
            TypeNameOutputLine { TypeName: "System.Collections.Immutable.SortedInt32KeyNode`1[[System.Collections.Immutable.ImmutableDictionary`2+HashBucket[[System.String, System.Private.CoreLib],[Microsoft.Build.Execution.ProjectMetadataInstance, Microsoft.Build]], System.Collections.Immutable]]" },
            {},
            {},
            {},
            {},
            {},
            {},
            {},
            {}
        ]);
    }
}