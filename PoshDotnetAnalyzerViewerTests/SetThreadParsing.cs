namespace PoshDotnetAnalyzerViewerTests;

public class SetThreadParsing
{
    public void Test()
    {
        var setThreadOutput = new[]
        {
            "> setthread -t", // or just setthread
            "*0 0x0001 (1)",
            " 1 0x0008 (8)",
            " 2 0x0009 (9)",
            " 3 0x000A (10)",
            " 4 0x000B (11)"
        };
    }

    public void Test2()
    {
        var setThreadVOutput = new[]
        {
            "> setthread -v",
            "*0 0x0001 (1)", // parse only these using regex?
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
    }
}