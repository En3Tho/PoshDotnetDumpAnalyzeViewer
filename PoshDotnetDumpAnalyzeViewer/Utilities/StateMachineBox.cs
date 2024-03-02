using System.Runtime.CompilerServices;

namespace PoshDotnetDumpAnalyzeViewer.Utilities;

internal abstract class StateMachineBox : IThreadPoolWorkItem
{
    public abstract Action MoveNextAction { get; }

    public void Execute()
    {
        MoveNextAction();
    }
}

internal sealed class StateMachineBox<T> : StateMachineBox where T : IAsyncStateMachine
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

internal static class Sentinel
{
    internal static readonly object Completed = new();
}