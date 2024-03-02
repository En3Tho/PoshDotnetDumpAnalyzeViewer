using PoshDotnetDumpAnalyzeViewer;
using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using Xunit;

namespace PoshDotnetAnalyzerViewerTests;

public class GCRootParsing
{
    [Fact]
    public void TestThatGCRootIsParsedCorrectly()
    {
        var output = new[]
        {
            "> gcroot 7ef6513ff0b0",
            "HandleTable:",
            "    00007f00b6771cf0 (strong handle)",
            "        -> 00007efc28036bc8 System.Object[]",
            "        -> 00007ef6283fa0a8 System.Threading.SemaphoreSlim",
            "        -> 00007ef653217720 System.Threading.SemaphoreSlim+TaskNode",
            "        -> 00007ef6532177b8 System.Threading.Tasks.Task+TwoTaskWhenAnyPromise<System.Threading.Tasks.Task>",
            "        -> 00007ef653217810 System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Boolean>+AsyncStateMachineBox<System.Threading.SemaphoreSlim+<WaitUntilCountOrTimeoutAsync>d__33>",
            "        -> 00007ef6283c87b0 System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>+AsyncStateMachineBox<MySqlConnector.Core.BackgroundConnectionResetHelper+<ReturnSessionsAsync>d__3>",
            "        -> 00007ef6283c8618 System.Collections.Generic.List<System.Threading.Tasks.Task<System.Boolean>>",
            "        -> 00007ef82e9ba800 System.Threading.Tasks.Task<System.Boolean>[]",
            "        -> 00007ef6513fefc8 System.Threading.Tasks.Task<System.Boolean>",
            "        -> 00007ef6513ff010 System.Threading.Tasks.Task+ContingentProperties",
            "        -> 00007ef6513ff060 System.Threading.Tasks.TaskExceptionHolder",
            "        -> 00007ef6513ff090 System.Collections.Generic.List<System.Runtime.ExceptionServices.ExceptionDispatchInfo>",
            "        -> 00007ef6513ff0b0 System.Runtime.ExceptionServices.ExceptionDispatchInfo[]",
            "",
            "Found 1 unique roots."
        };

        var lines = OutputParserExtensions.ParseAll<GCRootParser>(output, Commands.GCRoot);

        Assert.True(lines is
        [
            not (GCRootOutputLine or ObjectAddressOutputLine),
            not (GCRootOutputLine or ObjectAddressOutputLine),
            ObjectAddressOutputLine { Address: "00007f00b6771cf0" },
            GCRootOutputLine { Address: "00007efc28036bc8", TypeName: "System.Object[]" },
            GCRootOutputLine { Address: "00007ef6283fa0a8", TypeName: "System.Threading.SemaphoreSlim" },
            GCRootOutputLine { Address: "00007ef653217720", TypeName: "System.Threading.SemaphoreSlim+TaskNode" },
            GCRootOutputLine { Address: "00007ef6532177b8", TypeName: "System.Threading.Tasks.Task+TwoTaskWhenAnyPromise<System.Threading.Tasks.Task>" },
            GCRootOutputLine { Address: "00007ef653217810", TypeName: "System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Boolean>+AsyncStateMachineBox<System.Threading.SemaphoreSlim+<WaitUntilCountOrTimeoutAsync>d__33>" },
            GCRootOutputLine { Address: "00007ef6283c87b0", TypeName: "System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>+AsyncStateMachineBox<MySqlConnector.Core.BackgroundConnectionResetHelper+<ReturnSessionsAsync>d__3>" },
            GCRootOutputLine { Address: "00007ef6283c8618", TypeName: "System.Collections.Generic.List<System.Threading.Tasks.Task<System.Boolean>>" },
            GCRootOutputLine { Address: "00007ef82e9ba800", TypeName: "System.Threading.Tasks.Task<System.Boolean>[]" },
            GCRootOutputLine { Address: "00007ef6513fefc8", TypeName: "System.Threading.Tasks.Task<System.Boolean>" },
            GCRootOutputLine { Address: "00007ef6513ff010", TypeName: "System.Threading.Tasks.Task+ContingentProperties" },
            GCRootOutputLine { Address: "00007ef6513ff060", TypeName: "System.Threading.Tasks.TaskExceptionHolder" },
            GCRootOutputLine { Address: "00007ef6513ff090", TypeName: "System.Collections.Generic.List<System.Runtime.ExceptionServices.ExceptionDispatchInfo>" },
            GCRootOutputLine { Address: "00007ef6513ff0b0", TypeName: "System.Runtime.ExceptionServices.ExceptionDispatchInfo[]" },
            not (GCRootOutputLine or ObjectAddressOutputLine),
            not (GCRootOutputLine or ObjectAddressOutputLine)
        ]);
    }
}