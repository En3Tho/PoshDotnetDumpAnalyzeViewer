using PoshDotnetDumpAnalyzeViewer;
using Xunit;

namespace PoshDotnetAnalyzerViewerTests;

public class ClrStackParsing
{
    [Fact]
    public void TestThatClrStackOutputIsParsedCorrectly()
    {
        var output = new[]
        {
            "000000AD511FE590 00007FFF162883F9 RabbitMQ.Client.ConsumerWorkService+WorkPool.Loop()",
            "PARAMETERS:",
            "this (0x000000AD511FE5E0) = 0x000001c4c14c1600",
            "LOCALS:",
            "    <no data>"
        };

        var lines = OutputParserExtensions.ParseAll<ClrStackParser>(output, Commands.ClrStack);

        Assert.True(lines is [
            not ObjectAddressOutputLine,
            not ObjectAddressOutputLine,
            ObjectAddressOutputLine { Address.Span: "0x000001c4c14c1600" },
            not ObjectAddressOutputLine,
            not ObjectAddressOutputLine,
        ]);
    }
}