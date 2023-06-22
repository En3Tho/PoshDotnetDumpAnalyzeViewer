using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

[AsyncMethodBuilder(typeof(GuiCsTaskMethodBuilder))]
public sealed class UITask
{
    private object? _state;
    private Action _moveNext = null!;
    private Action? continuation;

    internal void SetResult()
    {
        _state = Sentinel.Completed;
        if (continuation is { })
            Application.MainLoop.Invoke(continuation);
    }

    internal void SetException(Exception exception) => _state = exception;

    internal void SetStateMachine<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
    {
        var box = new StateMachineBox<TStateMachine>(stateMachine);
        _moveNext = box.MoveNextAction;
    }

    internal void Start()
    {
        Application.MainLoop.Invoke(_moveNext);
    }

    internal void SetContinuation(Action cont)
    {
        continuation = cont;
    }

    public GuiCsTaskAwaiter GetAwaiter() => new(this);

    public readonly struct GuiCsTaskAwaiter : ICriticalNotifyCompletion
    {
        private readonly UITask _task;

        public GuiCsTaskAwaiter(UITask task)
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
                case null:
                    throw new("Synchronous awaits are not supported");
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
            Task.SetStateMachine(ref stateMachine);
            Task.Start();
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

        public UITask Task { get; }
    }
}