using System.Collections.Immutable;
using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using PoshDotnetDumpAnalyzeViewer.Views;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.OutputViewFactories;

public sealed record DumpDomainCommandViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandViewFactoryBase<DumpDomainParser>(
    MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpDomain);
}