using PoshDotnetDumpAnalyzeViewer;
using Xunit;

namespace PoshDotnetAnalyzerViewerTests;

public class SetThreadParsing
{
    [Fact]
    public void TestThatOsIdIsParsedCorrectly()
    {
        Assert.Matches(SetThread.OsIdParser, "*0 0x0001 (1)");
        Assert.Matches(SetThread.OsIdParser, " 1 0x000A (10)");
        Assert.Matches(SetThread.OsIdParser, " 50 0x025C (604)");

        var line1 = new SetThreadOutputLine("*0 0x0001 (1)");
        Assert.Equal("0x0001", line1.OsThreadId.ToString());
        var line2 = new SetThreadOutputLine(" 1 0x000A (10)");
        Assert.Equal("0x000A", line2.OsThreadId.ToString());
        var line3 = new SetThreadOutputLine(" 50 0x025C (604)");
        Assert.Equal("0x025C", line3.OsThreadId.ToString());
    }

    [Fact]
    public void TestThatSetThreadWithTFlagOutputIsParsedCorrectly()
    {
        var output = new[]
        {
            "> setthread -t", // or just setthread or just threads
            "*0 0x0001 (1)", // get hex and convert it to int if needed?
            " 1 0x0008 (8)",
            " 2 0x0009 (9)",
            " 3 0x000A (10)",
            " 4 0x000B (11)"
        };

        var parseResult = new SetThreadOutputParser().Parse("", output);
        var lines = parseResult.Lines;

        Assert.True(lines is
        [
            {},
            SetThreadOutputLine,
            SetThreadOutputLine,
            SetThreadOutputLine,
            SetThreadOutputLine,
            SetThreadOutputLine
        ]);
    }

    [Fact]
    public void TestThatSetThreadWithVFlagOutputIsParsedCorrectly()
    {
        var output = new[]
        {
            "> setthread -v",
            "*0 0x0001 (1)",
            "   IP  0x00007FCDA018D413",
            "   SP  0x00007FFF43CC37B8",
            "   FP  0x0000000000000080",
            "   TEB 0x0000000000000000",
            " 1 0x0008 (8)",
            "   IP  0x00007FCDA018D413",
            "   SP  0x00007FCD25AB3798",
            "   FP  0x00007FCD25AB3840",
            "   TEB 0x0000000000000000",
            " 2 0x0009 (9)",
            "   IP  0x00007FCDA018D413",
            "   SP  0x00007FCD25930478",
            "   FP  0x00007FCD25930530",
            "   TEB 0x0000000000000000"
        };

        var parseResult = new SetThreadOutputParser().Parse("", output);
        var lines = parseResult.Lines;

        Assert.True(lines is
        [
            { },
            SetThreadOutputLine,
            { },
            { },
            { },
            { },
            SetThreadOutputLine,
            { },
            { },
            { },
            { },
            SetThreadOutputLine,
            { },
            { },
            { },
            { }
        ]);
    }
}