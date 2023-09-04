using System.Management.Automation;
using PoshDotnetDumpAnalyzeViewer;

namespace PoshDotnetDumpAnalyzeViewerModule;

[Cmdlet(VerbsCommon.Get, "DotnetDumpAnalyzeViewer")]
public class GetDotnetDumpAnalyzeViewerCommand : PSCmdlet
{
    [Parameter(Position = 0, Mandatory = true)]
    [ValidateNotNullOrEmpty]
    public string AnalyzeArgs { get; init; } = null!;

    [Parameter]
    public string? FileName { get; init; } = null!;

    protected override void ProcessRecord()
    {
        try
        {
            var analyzeArgs = SessionState.Path.GetUnresolvedProviderPathFromPSPath(AnalyzeArgs);
            var fileName = FileName ?? App.DotnetDumpToolName;
            App.Run(fileName, analyzeArgs).GetAwaiter().GetResult();
        }
        finally
        {
            if (!Console.IsInputRedirected)
                Console.Write("\u001b[?1h\u001b[?1003l"); // fixes an issue with certain terminals, same as ocgv
        }
    }
}