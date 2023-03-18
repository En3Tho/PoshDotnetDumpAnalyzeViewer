using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

// Gui.cs specific tasks that schedule all continuations back to Driver thread
// TODO: Gui.cs has sync context too. Reuse that maybe?

internal abstract class StateMachineBox
{
    public abstract Action MoveNextAction { get; }
}

internal class StateMachineBox<T> : StateMachineBox where T : IAsyncStateMachine
{
    private T _machine;
    public override Action MoveNextAction { get; }

    public StateMachineBox(T machine)
    {
        _machine = machine;
        MoveNextAction = MoveNext;
    }

    private void MoveNext() => _machine.MoveNext();
}

[AsyncMethodBuilder(typeof(GuiCsTaskMethodBuilder))]
public class GuiCsTask
{
    private static readonly object _completedSentinel = new();
    internal object? _state;
    internal Action _moveNext = null!;
    internal Action? continuation;

    internal void SetResult()
    {
        _state = _completedSentinel;
        continuation?.Invoke();
    }

    internal void SetException(Exception exception) => _state = exception;

    internal void SetContinuation(Action cont)
    {
        continuation = () => Application.MainLoop.Invoke(cont);
        if (ReferenceEquals(_state, _completedSentinel)) // should not be possible?
            continuation();
    }

    public GuiCsTaskAwaiter GetAwaiter() => new(this);
}

public readonly struct GuiCsTaskAwaiter : ICriticalNotifyCompletion
{
    private readonly GuiCsTask _task;

    public GuiCsTaskAwaiter(GuiCsTask task)
    {
        _task = task;
    }

    public void OnCompleted(Action continuation)
    {
        _task.SetContinuation(continuation);
    }

    public void UnsafeOnCompleted(Action continuation)
    {
        _task.SetContinuation(continuation);
    }

    public bool IsCompleted => _task._state is {};

    public void GetResult()
    {
        switch (_task._state)
        {
            case Exception ex:
                ExceptionDispatchInfo.Throw(ex);
                break;
        }
    }
}

public readonly struct GuiCsTaskMethodBuilder
{
    public GuiCsTaskMethodBuilder()
    {
        Task = new();
    }

    public static GuiCsTaskMethodBuilder Create() => new();

    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    {
        // check if is on driver thread?
        var box = new StateMachineBox<TStateMachine>(stateMachine);
        Task._moveNext = box.MoveNextAction;
        Application.MainLoop.Invoke(box.MoveNextAction);
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
    }

    public void SetException(Exception exception) => Task.SetException(exception);

    public void SetResult() => Task.SetResult();

    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        awaiter.OnCompleted(Task._moveNext);
    }

    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter,
        ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        awaiter.OnCompleted(Task._moveNext);
    }

    public GuiCsTask Task { get; }
}