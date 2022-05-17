using System.Threading.Channels;
using En3Tho.Extensions.DependencyInjection;

namespace PoshDotnetDumpAnalyzeViewer;

public record CommandQueue2(IServiceProvider ServiceProvider, Action<Exception> ExceptionHandler)
{
    private readonly Channel<Func<ValueTask>> _channel =
        Channel.CreateUnbounded<Func<ValueTask>>(new() { SingleReader = true });

    public void SendCommand(Func<ValueTask> command)
    {
        _channel.Writer.TryWrite(command);
    }

    public void Start(CancellationToken token) => Task.Run(async () =>
    {
        var reader = _channel.Reader;
        await foreach (var command in reader.ReadAllAsync(token))
        {
            try
            {
                await command();
            }
            catch (Exception exn)
            {
                ExceptionHandler(exn);
            }
        }
    }, token);
}

public static class ServiceProviderCommandQueueExtensions
{
    public static void SendCommand<T1>(this CommandQueue2 @this, Func<T1, ValueTask> command)
        where T1 : notnull
    {
        @this.SendCommand(() => @this.ServiceProvider.RunAsync(command));
    }

    public static void SendCommand<T1, T2>(this CommandQueue2 @this, Func<T1, T2, ValueTask> command)
        where T1 : notnull
        where T2 : notnull
    {
        @this.SendCommand(() => @this.ServiceProvider.RunAsync(command));
    }

    public static void SendCommand<T1, T2, T3>(this CommandQueue2 @this, Func<T1, T2, T3, ValueTask> command)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
    {
        @this.SendCommand(() => @this.ServiceProvider.RunAsync(command));
    }

    public static void SendCommand<T1, T2, T3, T4>(this CommandQueue2 @this, Func<T1, T2, T3, T4, ValueTask> command)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
    {
        @this.SendCommand(() => @this.ServiceProvider.RunAsync(command));
    }

    public static void SendCommand<T1, T2, T3, T4, T5>(this CommandQueue2 @this, Func<T1, T2, T3, T4, T5, ValueTask> command)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where T5 : notnull
    {
        @this.SendCommand(() => @this.ServiceProvider.RunAsync(command));
    }
}