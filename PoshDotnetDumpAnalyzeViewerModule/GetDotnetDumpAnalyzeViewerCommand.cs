using System.Management.Automation;
using PoshDotnetDumpAnalyzeViewer;

namespace PoshDotnetDumpAnalyzeViewerModule;

[Cmdlet(VerbsCommon.Get, "DotnetDumpAnalyzeViewer")]
public class GetDotnetDumpAnalyzeViewerCommand : Cmdlet
{
    [Parameter(Position = 0, Mandatory = true)]
    [ValidateNotNullOrEmpty]
    public string AnalyzeArgs { get; set; } = null!;

    protected override void ProcessRecord()
    {
        try
        {
            App.Run(AnalyzeArgs).GetAwaiter().GetResult();
        }
        finally
        {
            if (!Console.IsInputRedirected)
                Console.Write("\u001b[?1h\u001b[?1003l"); // fixes an issue with certain terminals, same as ocgv
        }
    }
}