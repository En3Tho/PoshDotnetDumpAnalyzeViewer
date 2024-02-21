using PoshDotnetDumpAnalyzeViewer;
using Xunit;

namespace PoshDotnetAnalyzerViewerTests;

public class SetThreadParsing
{
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
            " 4 0x000B (11)",
            "*0 0x838C (33676)"
        };

        var lines = OutputParserExtensions.ParseAll<SetThreadParser>(output, Commands.SetThread);

        Assert.True(lines is
        [
            not SetThreadOutputLine,
            SetThreadOutputLine { OsThreadId.Span: "0x0001" },
            SetThreadOutputLine { OsThreadId.Span: "0x0008" },
            SetThreadOutputLine { OsThreadId.Span: "0x0009" },
            SetThreadOutputLine { OsThreadId.Span: "0x000A" },
            SetThreadOutputLine { OsThreadId.Span: "0x000B" },
            SetThreadOutputLine { OsThreadId.Span: "0x838C" },
        ]);

        var idsOsThreadIds = lines.Select(l => l as SetThreadOutputLine is {} line ? line.GetIntOsThreadId() : -1).ToArray();
        Assert.True(idsOsThreadIds is
            [
                -1,
                1,
                8,
                9,
                10,
                11,
                33676
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

        var lines = OutputParserExtensions.ParseAll<SetThreadParser>(output, Commands.SetThread);

        Assert.True(lines is
        [
            not SetThreadOutputLine,
            SetThreadOutputLine { OsThreadId.Span: "0x0001" },
            not SetThreadOutputLine,
            not SetThreadOutputLine,
            not SetThreadOutputLine,
            not SetThreadOutputLine,
            SetThreadOutputLine { OsThreadId.Span: "0x0008" },
            not SetThreadOutputLine,
            not SetThreadOutputLine,
            not SetThreadOutputLine,
            not SetThreadOutputLine,
            SetThreadOutputLine { OsThreadId.Span: "0x0009" },
            not SetThreadOutputLine,
            not SetThreadOutputLine,
            not SetThreadOutputLine,
            not SetThreadOutputLine
        ]);
    }
}