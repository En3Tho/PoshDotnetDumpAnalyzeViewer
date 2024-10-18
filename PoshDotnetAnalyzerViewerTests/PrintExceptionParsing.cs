using PoshDotnetDumpAnalyzeViewer;
using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using Xunit;

namespace PoshDotnetAnalyzerViewerTests;

public class PrintExceptionParsing
{
    [Fact]
    public void TestThatPrintExceptionOutputIsParsedCorrectly()
    {
        var output = new[]
        {
            "> printexception 00007EF62A1E2E60",
            "Exception object: 00007ef62a1e2e60",
            "Exception type:   System.Net.WebSockets.WebSocketException",
            "Message:          The remote party closed the WebSocket connection without completing the close handshake.",
            "InnerException:   <none>",
            "StackTrace (generated):",
            "   SP               IP               Function",
            "   00007EF5E8C16000 00007F00496C68AE System.Net.WebSockets.dll!System.Net.WebSockets.ManagedWebSocket.ThrowIfEOFUnexpected(Boolean)+0xae",
            "   00007EF5E8C16030 00007F00490D9350 System.Net.WebSockets.dll!System.Net.WebSockets.ManagedWebSocket+<EnsureBufferContainsAsync>d__76.MoveNext()+0x3f0",
            "   00007EF5E8C146C0 00007F0048698ADC System.Private.CoreLib.dll!System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()+0x1c",
            "   00007EF5E8C146D0 00007F00487D9A3C System.Private.CoreLib.dll!System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(System.Threading.Tasks.Task)+0xcc",
            "   00007EF5E8C146F0 00007F00491ACC87 System.Net.WebSockets.dll!System.Net.WebSockets.ManagedWebSocket+<ReceiveAsyncPrivate>d__66`2[[System.Net.WebSockets.ManagedWebSocket+ValueWebSocketReceiveResultGetter, System.Net.WebSockets],[System.Net.WebSockets.ValueWebSocketReceiveResult, System.Net.WebSockets]].MoveNext()+0x387",
            "",
            "StackTraceString: <none>",
            "HResult: 80004005",
        };

        var lines = OutputParserExtensions.ParseAll<PrintExceptionParser>(output, Commands.ObjSize);

        Assert.True(lines is
        [
            not (ObjectAddressOutputLine or ExceptionObjectAddressOutputLine or TypeNameOutputLine),
            ExceptionObjectAddressOutputLine { Address: "00007ef62a1e2e60" },
            TypeNameOutputLine { TypeName: "System.Net.WebSockets.WebSocketException" },
            not (ObjectAddressOutputLine or ExceptionObjectAddressOutputLine or TypeNameOutputLine),
            not (ObjectAddressOutputLine or ExceptionObjectAddressOutputLine or TypeNameOutputLine),
            not (ObjectAddressOutputLine or ExceptionObjectAddressOutputLine or TypeNameOutputLine),
            not (ObjectAddressOutputLine or ExceptionObjectAddressOutputLine or TypeNameOutputLine),
            not (ObjectAddressOutputLine or ExceptionObjectAddressOutputLine or TypeNameOutputLine),
            not (ObjectAddressOutputLine or ExceptionObjectAddressOutputLine or TypeNameOutputLine),
            not (ObjectAddressOutputLine or ExceptionObjectAddressOutputLine or TypeNameOutputLine),
            not (ObjectAddressOutputLine or ExceptionObjectAddressOutputLine or TypeNameOutputLine),
            not (ObjectAddressOutputLine or ExceptionObjectAddressOutputLine or TypeNameOutputLine),
            not (ObjectAddressOutputLine or ExceptionObjectAddressOutputLine or TypeNameOutputLine),
            not (ObjectAddressOutputLine or ExceptionObjectAddressOutputLine or TypeNameOutputLine),
            not (ObjectAddressOutputLine or ExceptionObjectAddressOutputLine or TypeNameOutputLine),
        ]);
    }

    [Fact]
    public void TestThatPrintExceptionOOutputIsParsedCorrectly2()
    {
        var output = new[]
        {
            "> printexception 7ef62a1e2fc8",
            "Exception object: 00007ef62a1e2fc8",
            "Exception type:   System.OperationCanceledException",
            "Message:          Aborted",
            "InnerException:   System.Net.WebSockets.WebSocketException, Use printexception 00007EF62A1E2E60 to see more.",
            "StackTrace (generated):",
            "   SP               IP               Function",
            "   00007EF5E8C13210 00007F00491AE15A System.Net.WebSockets.dll!System.Net.WebSockets.ManagedWebSocket+<ReceiveAsyncPrivate>d__66`2[[System.Net.WebSockets.ManagedWebSocket+ValueWebSocketReceiveResultGetter, System.Net.WebSockets],[System.Net.WebSockets.ValueWebSocketReceiveResult, System.Net.WebSockets]].MoveNext()+0x185a",
            "   00007EF5E8C13510 00007F0048698ADC System.Private.CoreLib.dll!System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()+0x1c",
            "   00007EF5E8C13520 00007F00487D99C0 System.Private.CoreLib.dll!System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(System.Threading.Tasks.Task)+0x50",
            "   00007EF5E8C13540 00007F0047DE8E48 HotChocolate.AspNetCore.dll!HotChocolate.AspNetCore.Subscriptions.WebSocketConnection+<ReceiveAsync>d__21.MoveNext()+0x2d8",
            "",
            "StackTraceString: <none>",
            "HResult: 8013153b",
        };

        var lines = OutputParserExtensions.ParseAll<PrintExceptionParser>(output, Commands.ObjSize);

        Assert.True(lines is
        [
            not (ObjectAddressOutputLine or ExceptionObjectAddressOutputLine or TypeNameOutputLine),
            ExceptionObjectAddressOutputLine { Address: "00007ef62a1e2fc8" },
            TypeNameOutputLine { TypeName: "System.OperationCanceledException" },
            not (ObjectAddressOutputLine or ExceptionObjectAddressOutputLine or TypeNameOutputLine),
            ExceptionObjectAddressOutputLine { Address: "00007EF62A1E2E60" },
            not (ObjectAddressOutputLine or ExceptionObjectAddressOutputLine or TypeNameOutputLine),
            not (ObjectAddressOutputLine or ExceptionObjectAddressOutputLine or TypeNameOutputLine),
            not (ObjectAddressOutputLine or ExceptionObjectAddressOutputLine or TypeNameOutputLine),
            not (ObjectAddressOutputLine or ExceptionObjectAddressOutputLine or TypeNameOutputLine),
            not (ObjectAddressOutputLine or ExceptionObjectAddressOutputLine or TypeNameOutputLine),
            not (ObjectAddressOutputLine or ExceptionObjectAddressOutputLine or TypeNameOutputLine),
            not (ObjectAddressOutputLine or ExceptionObjectAddressOutputLine or TypeNameOutputLine),
            not (ObjectAddressOutputLine or ExceptionObjectAddressOutputLine or TypeNameOutputLine),
            not (ObjectAddressOutputLine or ExceptionObjectAddressOutputLine or TypeNameOutputLine),
        ]);
    }
}