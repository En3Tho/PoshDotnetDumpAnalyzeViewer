﻿using PoshDotnetDumpAnalyzeViewer;
using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using Xunit;

namespace PoshDotnetAnalyzerViewerTests;

public class ClrThreadsParsing
{
    [Fact]
    public void TestThatClrThreadsOutputIsParsedCorrectly()
    {
        var output = new[]
        {
            "> clrthreads",
            "ThreadCount:      42",
            "UnstartedThread:  0",
            "BackgroundThread: 37",
            "PendingThread:    0",
            "DeadThread:       0",
            "Hosted Runtime:   no",
            "                                                                                                            Lock  ",
            " DBG   ID     OSID ThreadOBJ           State GC Mode     GC Alloc Context                  Domain           Count Apt Exception",
            "   0    1        1 00007FCD25261010  2020020 Preemptive  0000000000000000:0000000000000000 00007FCD9FE44060 -00001 Ukn ",
            "   9    2       10 00007FC30CD189B0    21220 Preemptive  00007FC33D9F09D0:00007FC33D9F29A0 00007FCD9FE44060 -00001 Ukn (Finalizer) ",
            "  10    4       12 00007FC30C016CE0    21220 Preemptive  0000000000000000:0000000000000000 00007FCD9FE44060 -00001 Ukn ",
            "  11    5       13 00007FC30C018670  1020220 Preemptive  00007FC33DD3AE18:00007FC33DD3C9A0 00007FCD9FE44060 -00001 Ukn (Threadpool Worker) ",
            "Special"
        };

        var lines = OutputParserExtensions.ParseAll<ClrThreadsParser>(output, Commands.ClrThreads);

        Assert.True(lines is [
            not ClrThreadsOutputLine,
            not ClrThreadsOutputLine,
            not ClrThreadsOutputLine,
            not ClrThreadsOutputLine,
            not ClrThreadsOutputLine,
            not ClrThreadsOutputLine,
            not ClrThreadsOutputLine,
            not ClrThreadsOutputLine,
            not ClrThreadsOutputLine,
            ClrThreadsOutputLine { OsThreadId: "1", ClrThreadId: "1", ThreadState: "2020020" },
            ClrThreadsOutputLine { OsThreadId: "10", ClrThreadId: "2", ThreadState: "21220" },
            ClrThreadsOutputLine { OsThreadId: "12", ClrThreadId: "4", ThreadState: "21220" },
            ClrThreadsOutputLine { OsThreadId: "13", ClrThreadId: "5", ThreadState: "1020220" },
            not ClrThreadsOutputLine
        ]);
    }
}