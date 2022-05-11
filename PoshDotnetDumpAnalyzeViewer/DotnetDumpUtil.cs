using System.Diagnostics;

namespace PoshDotnetDumpAnalyzeViewer;

public static class ProcessUtil
{
    public static async Task<Process> StartDotnetDumpAnalyze(string analyzeArgs)
    {
        var dotnetDumpStartInfo = new ProcessStartInfo
        {
            FileName =
                "dotnet-dump",
            Arguments = $"analyze {analyzeArgs}",
            RedirectStandardError = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true
        };

        var dotnetDump = Process.Start(dotnetDumpStartInfo);

        if (dotnetDump == null)
        {
            throw new("Unable to start dotnet dump process");
        }

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

        dotnetDump.Exited += (_, _) =>
        {
            Environment.Exit(0);
        };

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

public class DotnetDumpAnalyzeBridge
{
    private readonly Process _dotnetDump;
    private readonly CancellationToken _cancellationToken;

    public DotnetDumpAnalyzeBridge(Process dotnetDump, CancellationToken cancellationToken)
    {
        _dotnetDump = dotnetDump;
        _cancellationToken = cancellationToken;
    }

    public async Task<CommandOutput<OutputLine>> PerformCommand<TOutputParser>(string command)
        where TOutputParser : IOutputParser, new()
    {
        var parser = new TOutputParser();

        List<string> values = new(256);

        await _dotnetDump.StandardInput.WriteLineAsync(command);

        while (true)
        {
            var line = await _dotnetDump.StandardOutput.ReadLineAsync();
            if (line is Constants.EndCommandErrorAnchor)
            {
                await foreach (var error in _dotnetDump.StandardError.ReadAllLinesToEndAsync().WithCancellation(_cancellationToken))
                {
                    values.Add(error);
                }

                return parser.Parse(command, values.ToArray(), false);
            }

            if (line is null or Constants.EndCommandOutputAnchor)
                break;

            values.Add(line);
        }

        return parser.Parse(command, values.ToArray(), true);
    }
}

public record struct CommandOutput<T>(string Command, bool IsOk, T[] Lines)
    where T : IOutputLine;