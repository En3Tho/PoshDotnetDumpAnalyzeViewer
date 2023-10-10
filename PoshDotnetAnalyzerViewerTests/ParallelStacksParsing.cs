using PoshDotnetDumpAnalyzeViewer;
using Xunit;

namespace PoshDotnetAnalyzerViewerTests;

public class ParallelStacksParsing
{
    // note: this is a reversed output
    private static readonly string[] Output =
    {
        "==> 6 threads with 4 roots",
        "",
        "",
        "    1 System.Threading.PortableThreadPool+GateThread.GateThreadStart()",
        "    1 System.Threading.WaitHandle.WaitOneNoCheck(Int32)",
        " ~~~~ d4f8",
        "________________________________________________",
        "",
        "",
        "    3 System.Threading.PortableThreadPool+WorkerThread.WorkerThreadStart()",
        "    3 System.Threading.ThreadPoolWorkQueue.Dispatch()",
        "         2 System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task ByRef, Thread)",
        "         2 System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread, ExecutionContext, ContextCallback, Object)",
        "              1 Terminal.Gui.WindowsMainLoop.CheckWinChange()",
        "              1 Terminal.Gui.WindowsMainLoop.WaitWinChange()",
        "              1 System.Threading.Thread.Sleep(Int32)",
        "           ~~~~ 8a0c",
        "              1 Terminal.Gui.WindowsMainLoop.WindowsInputHandler()",
        "              1 Terminal.Gui.WindowsConsole.ReadConsoleInput()",
        "           ~~~~ bec4",
        "         1 System.Threading.TimerQueue.FireNextTimers()",
        "         1 System.Threading.TimerQueueTimer.Fire(Boolean)",
        "         1 System.Threading.Tasks.Task+DelayPromise.CompleteTimedOut()",
        "         1 System.Threading.Tasks.Task.TrySetResult()",
        "         1 System.Threading.Tasks.Task.RunContinuations(Object)",
        "         1 System.Threading.Tasks.AwaitTaskContinuation.RunOrScheduleAction(IAsyncStateMachineBox, Boolean)",
        "         1 System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.__Canon>+AsyncStateMachineBox<System.__Canon>.MoveNext(Thread)",
        "         1 System.Threading.ExecutionContext.RunInternal(ExecutionContext, ContextCallback, Object)",
        "         1 PoshDotnetDumpAnalyzeViewer.ProcessUtil+<StartDotnetDumpAnalyze>d__0.MoveNext()",
        "         1 System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.__Canon>.SetExistingTaskResult(Task<__Canon>, __Canon)",
        "         1 System.Threading.Tasks.Task<System.__Canon>.TrySetResult(__Canon)",
        "         1 System.Threading.Tasks.Task.RunContinuations(Object)",
        "         1 System.Threading.Tasks.AwaitTaskContinuation.RunOrScheduleAction(IAsyncStateMachineBox, Boolean)",
        "         1 System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>+AsyncStateMachineBox<System.__Canon>.MoveNext(Thread)",
        "         1 System.Threading.ExecutionContext.RunInternal(ExecutionContext, ContextCallback, Object)",
        "         1 PoshDotnetDumpAnalyzeViewer.App+<Run>d__1.MoveNext()",
        "         1 Terminal.Gui.Application.Run(Toplevel, Func<Exception,Boolean>)",
        "         1 Terminal.Gui.Application.RunLoop(RunState, Boolean)",
        "         1 Terminal.Gui.WindowsMainLoop.EventsPending(Boolean)",
        "         1 System.Threading.ManualResetEventSlim.Wait(Int32, CancellationToken)",
        "      ~~~~ bbe4",
        "________________________________________________",
        "",
        "",
        "    1 System.Threading.TimerQueue.TimerThread()",
        "    1 System.Threading.WaitHandle.WaitOneNoCheck(Int32)",
        " ~~~~ 46ec",
        "________________________________________________",
        "",
        "",
        "    1 Program.<Main>(String[])",
        "    1 System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task)",
        "    1 System.Threading.Tasks.Task.InternalWaitCore(Int32, CancellationToken)",
        "    1 System.Threading.Tasks.Task.SpinThenBlockingWait(Int32, CancellationToken)",
        "    1 System.Threading.ManualResetEventSlim.Wait(Int32, CancellationToken)",
        " ~~~~ 24d0",
        "________________________________________________",
        "",
        "> pstacks -a",
    };

    [Fact]
    public void TestThatObjSizeDefaultOutputIsParsedCorrectly()
    {
        var lines = OutputParserExtensions.ParseAll<ParallelStacksParser>(Output, Commands.ParallelStacks);

        foreach (var line in lines)
        {
            Assert.True(line is ParallelStacksOutputLine);
        }
    }

    [Fact]
    public void TestThatThreadCountParsingWorksCorrectly()
    {
        var output = new[]
        {
            "==> 6 threads with 4 roots",
            "",
            "",
            "    1 System.Threading.PortableThreadPool+GateThread.GateThreadStart()",
            "    1 System.Threading.WaitHandle.WaitOneNoCheck(Int32)",
            " ~~~~ d4f8",
            "________________________________________________",
        };

        Assert.False(ParallelStacksParser.TryParseThreadCount(output[0], out _));
        Assert.False(ParallelStacksParser.TryParseThreadCount(output[1], out _));
        Assert.False(ParallelStacksParser.TryParseThreadCount(output[2], out _));
        Assert.True(ParallelStacksParser.TryParseThreadCount(output[3], out var tc1) && tc1 == 1);
        Assert.True(ParallelStacksParser.TryParseThreadCount(output[4], out var tc2) && tc2 == 1);
        Assert.False(ParallelStacksParser.TryParseThreadCount(output[5], out _));
        Assert.False(ParallelStacksParser.TryParseThreadCount(output[6], out _));
    }

    [Fact]
    public void TestThatIsThreadNamesWorksCorrectly()
    {
        var output = new[]
        {
            "==> 6 threads with 4 roots",
            "",
            "",
            "    1 System.Threading.PortableThreadPool+GateThread.GateThreadStart()",
            "    1 System.Threading.WaitHandle.WaitOneNoCheck(Int32)",
            " ~~~~ d4f8",
            "________________________________________________",
        };

        Assert.False(ParallelStacksParser.IsThreadNames(output[0]));
        Assert.False(ParallelStacksParser.IsThreadNames(output[1]));
        Assert.False(ParallelStacksParser.IsThreadNames(output[2]));
        Assert.False(ParallelStacksParser.IsThreadNames(output[3]));
        Assert.False(ParallelStacksParser.IsThreadNames(output[4]));
        Assert.True(ParallelStacksParser.IsThreadNames(output[5]));
        Assert.False(ParallelStacksParser.IsThreadNames(output[6]));
    }

    [Fact]
    public void TestThatShrinkingParallelStacksOutputWorksCorrectly()
    {
        var shrinkedOutput = ParallelStacksOutputFactory.ShrinkParallelStacksOutput(Output);

        Assert.True(shrinkedOutput is
        [
            "==> 6 threads with 4 roots",
            "",
            "",
            "    1 System.Threading.PortableThreadPool+GateThread.GateThreadStart()",
            "    1 System.Threading.WaitHandle.WaitOneNoCheck(Int32)",
            " ~~~~ d4f8",
            "________________________________________________",
            "",
            "",
            "    3 System.Threading.ThreadPoolWorkQueue.Dispatch()",
            "    3 System.Threading.ThreadPoolWorkQueue.Dispatch()",
            "         2 System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task ByRef, Thread)",
            "         2 System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread, ExecutionContext, ContextCallback, Object)",
            "              1 Terminal.Gui.WindowsMainLoop.CheckWinChange()",
            "              1 System.Threading.Thread.Sleep(Int32)",
            "           ~~~~ 8a0c",
            "              1 Terminal.Gui.WindowsMainLoop.WindowsInputHandler()",
            "              1 Terminal.Gui.WindowsConsole.ReadConsoleInput()",
            "           ~~~~ bec4",
            "         1 System.Threading.TimerQueue.FireNextTimers()",
            "         1 System.Threading.ManualResetEventSlim.Wait(Int32, CancellationToken)",
            "      ~~~~ bbe4",
            "________________________________________________",
            "",
            "",
            "    1 System.Threading.TimerQueue.TimerThread()",
            "    1 System.Threading.WaitHandle.WaitOneNoCheck(Int32)",
            " ~~~~ 46ec",
            "________________________________________________",
            "",
            "",
            "    1 Program.<Main>(String[])",
            "    1 System.Threading.ManualResetEventSlim.Wait(Int32, CancellationToken)",
            " ~~~~ 24d0",
            "________________________________________________",
            "",
            "> pstacks -a",
        ]);
    }
}