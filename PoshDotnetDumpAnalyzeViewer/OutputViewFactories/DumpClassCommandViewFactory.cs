using System.Collections.Immutable;
using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using PoshDotnetDumpAnalyzeViewer.Views;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.OutputViewFactories;

public sealed record DumpClassCommandViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandViewFactoryBase<DumpClassParser>(
    MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.DumpClass);
}