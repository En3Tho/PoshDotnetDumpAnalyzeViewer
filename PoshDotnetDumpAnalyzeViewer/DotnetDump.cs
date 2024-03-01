using System.Diagnostics;

namespace PoshDotnetDumpAnalyzeViewer;

public static class ProcessUtil
{
    public static async Task<Process> StartDotnetDumpAnalyze(string fileName, string analyzeArgs)
    {
        var dotnetDumpStartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = $"analyze \"{analyzeArgs}\"",
            WorkingDirectory = Environment.CurrentDirectory,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true
        };

        var dotnetDump = Process.Start(dotnetDumpStartInfo);

        if (dotnetDump == null)
        {
            throw new("Unable to start dotnet-dump process");
        }

        // not sure how to reliably wait for errors?
        // should we wait instead for stdoutput?
        await Task.Delay(1000);

        if (dotnetDump.HasExited)
        {
            if (await dotnetDump.StandardError.ReadLineAsync() is { } errorMessage)
                throw new(errorMessage);
            throw new($"dotnet dump exited unexpectedly with code: {dotnetDump.ExitCode}");
        }

        var messages = new List<string>();

        await foreach (var message in dotnetDump.StandardOutput.ReadAllLinesToEndAsync())
        {
            if (message is Constants.EndCommandErrorAnchor)
                throw new(string.Join(" ,", messages));
            if (message is Constants.EndCommandOutputAnchor)
                break;
            messages.Add(message);
        }

        return dotnetDump;
    }
}

public static class Constants
{
    public const string EndCommandOutputAnchor = "<END_COMMAND_OUTPUT>";
    public const string EndCommandErrorAnchor = "<END_COMMAND_ERROR>";
}

public static class StreamReaderExtensions
{
    public static async IAsyncEnumerable<string> ReadAllLinesToEndAsync(this StreamReader @this)
    {
        while (!@this.EndOfStream)
        {
            if (await @this.ReadLineAsync() is { } line)
                yield return line;
        }
    }
}

public class DotnetDump(Process dotnetDump, CancellationToken cancellationToken)
{
    private readonly SemaphoreSlim _semaphore = new(0, 1);

    public async Task<(string[] Output, bool IsOk)> Run(string command)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            List<string> values = new(8192);

            await dotnetDump.StandardInput.WriteLineAsync(command);

            await foreach (var line in dotnetDump.StandardOutput.ReadAllLinesToEndAsync()
                               .WithCancellation(cancellationToken))
            {
                if (line.EndsWith(Constants.EndCommandErrorAnchor, StringComparison.Ordinal))
                {
                    return (values.ToArray(), false);
                }

                if (line.EndsWith(Constants.EndCommandOutputAnchor, StringComparison.Ordinal))
                    break;

                values.Add(line);
            }

            return (values.ToArray(), true);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

public record struct CommandOutput(string Command, string[] Lines);