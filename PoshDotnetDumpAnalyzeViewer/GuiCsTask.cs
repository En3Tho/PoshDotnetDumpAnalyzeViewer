using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

// It looks like it works but I'm not sure ;D
public readonly struct GuiCsTaskAwaiter : ICriticalNotifyCompletion
{
    private readonly GuiCsTask _task;

    public GuiCsTaskAwaiter(GuiCsTask task)
    {
        _task = task;
    }

    public void OnCompleted(Action continuation)
    {
        Application.MainLoop.Invoke(continuation);
    }

    public void UnsafeOnCompleted(Action continuation)
    {
        Application.MainLoop.Invoke(continuation);
    }

    public bool IsCompleted => _task._state != null;

    public void GetResult()
    {
        if (_task._state is Exception ex)
            ExceptionDispatchInfo.Throw(ex);
    }
}

[AsyncMethodBuilder(typeof(GuiCsTaskMethodBuilder))]
public class GuiCsTask : INotifyCompletion
{
    private static readonly object _completedSentinel = new();
    internal object? _state;

    public void OnCompleted(Action continuation)
    {
        Application.MainLoop.Invoke(continuation);
    }

    internal void SetResult() => _state = _completedSentinel;

    internal void SetException(Exception exception) => _state = exception;
    public GuiCsTaskAwaiter GetAwaiter() => new(this);
}

public class GuiCsTaskMethodBuilder
{
    public GuiCsTaskMethodBuilder()
    {
        Task = new();
    }

    public static GuiCsTaskMethodBuilder Create() => new();

    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    {
        stateMachine.MoveNext();
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine) { }

    public void SetException(Exception exception) => Task.SetException(exception);

    public void SetResult() => Task.SetResult();

    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
        =>
            awaiter.OnCompleted(stateMachine.MoveNext);

    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter,
        ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
        =>
            awaiter.OnCompleted(stateMachine.MoveNext);

    public GuiCsTask Task { get; }
}