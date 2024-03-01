using PoshDotnetDumpAnalyzeViewer;
using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using Xunit;

namespace PoshDotnetAnalyzerViewerTests;

public class DumpExceptionsParsing
{
    [Fact]
    public void TestThatDumpExceptionsParsingOutputIsParsedCorrectly()
    {
        var output = new[]
        {
            "> dumpexceptions",
            "Address      MethodTable Message Name",
            "01e422d00788     7ff8b7357890 StackExchange.Redis.RedisConnectionException",
            "        Message: No connection is available to service this operation: GET /_a2/workspace/display/11; SocketFailure on 127.0.0.1:6380/Subscription, origin: Error, input-buffer: 0, outstanding: 0, last-read: 1s ago, last-write: 1s ago, unanswered-write: 1309641s ago, keep-alive: 60s, pending: 0, state: Connecting, last-heartbeat: never, last-mbeat: -1s ago, global: 1s ago, mgr: Inactive, err: never; IOCP: (Busy=0,Free=1000,Min=32,Max=1000), WORKER: (Busy=1,Free=32766,Min=32,Max=32767), Local-CPU: n/a",
            "        StackFrame:",
            "01e462a93fc8     7ff90fec88d8 System.Net.Sockets.SocketException",
            "        Message: No connection could be made because the target machine actively refused it",
            "        StackFrame: System.Net.Sockets.Socket.InternalEndConnect(System.IAsyncResult)",
            "",
            "    Total: 2 objects"
        };

        var lines = OutputParserExtensions.ParseAll<DumpExceptionsParser>(output, Commands.DumpExceptions);

        Assert.True(lines is
        [
            not DumpExceptionsOutputLine,
            not DumpExceptionsOutputLine,
            DumpExceptionsOutputLine { Address.Span: "01e422d00788", MethodTable.Span: "7ff8b7357890", TypeName.Span: "StackExchange.Redis.RedisConnectionException" },
            not DumpExceptionsOutputLine,
            not DumpExceptionsOutputLine,
            DumpExceptionsOutputLine { Address.Span: "01e462a93fc8", MethodTable.Span: "7ff90fec88d8", TypeName.Span: "System.Net.Sockets.SocketException" },
            not DumpExceptionsOutputLine,
            not DumpExceptionsOutputLine,
            not DumpExceptionsOutputLine,
            not DumpExceptionsOutputLine,
        ]);
    }
}

