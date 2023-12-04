using System.Runtime.CompilerServices;

namespace PoshDotnetDumpAnalyzeViewer;

[AsyncMethodBuilder(typeof(UITaskMethodBuilder))]
public readonly struct UITask
{
    private readonly Task _task;

    public UITask(Task task)
    {
        _task = task;
    }

    public TaskAwaiter GetAwaiter() => _task.GetAwaiter();
}

public static class GuiCsSynchronizationContext
{
    public static SynchronizationContext Context { get; private set; } = null!;

    public static void SetSynchronizationContext(SynchronizationContext context) => Context = context;
}

public struct FakeAwaiter : ICriticalNotifyCompletion
{
    public Action action;

    public void OnCompleted(Action continuation)
    {
        action = continuation;
    }

    public void UnsafeOnCompleted(Action continuation)
    {
        action = continuation;
    }
}

public struct UITaskMethodBuilder
{
    private AsyncTaskMethodBuilder _methodBuilder;
    public static UITaskMethodBuilder Create() => new() { _methodBuilder = AsyncTaskMethodBuilder.Create() };

    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    {
        var ctx = GuiCsSynchronizationContext.Context;
        if (ReferenceEquals(SynchronizationContext.Current, ctx))
        {
            _methodBuilder.Start(ref stateMachine);
        }
        else
        {
            var fakeAwaiter = new FakeAwaiter();
            _methodBuilder.AwaitUnsafeOnCompleted(ref fakeAwaiter, ref stateMachine);
            ctx.Post(obj => ((Action)obj!)(), fakeAwaiter.action);
        }
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
    }

    public void SetException(Exception exception) => _methodBuilder.SetException(exception);

    public void SetResult() => _methodBuilder.SetResult();

    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        _methodBuilder.AwaitOnCompleted(ref awaiter, ref stateMachine);
    }

    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter,
        ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        _methodBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
    }

    public UITask Task => new(_methodBuilder.Task);

}