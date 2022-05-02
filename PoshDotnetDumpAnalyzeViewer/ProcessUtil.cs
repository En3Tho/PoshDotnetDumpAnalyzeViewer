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

        // skip first messages
        while (await dotnetDump.StandardOutput.ReadLineAsync() is not (null or Constants.EndCommandOutputAnchor))
        {
        }


        return dotnetDump;
    }
}