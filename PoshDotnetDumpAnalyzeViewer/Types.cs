using System.Diagnostics;

namespace PoshDotnetDumpAnalyzeViewer;

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

    public async Task<CommandOutput> PerformCommand(string command)
    {
        List<string> values = new(256);

        await _dotnetDump.StandardInput.WriteLineAsync(command.AsMemory(), _cancellationToken);

        while (true)
        {
            var line = await _dotnetDump.StandardOutput.ReadLineAsync();
            if (line is Constants.EndCommandErrorAnchor)
            {
                await foreach (var error in _dotnetDump.StandardError.ReadAllLinesToEndAsync().WithCancellation(_cancellationToken))
                {
                    values.Add(error);
                }

                return new(false, values.ToArray());
            }

            if (line is null or Constants.EndCommandOutputAnchor)
                break;

            values.Add(line);
        }

        return new (true, values.ToArray());
    }
}

public record struct CommandOutput(bool IsOk, string[] Output);