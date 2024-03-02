using System.Collections.Immutable;
using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using PoshDotnetDumpAnalyzeViewer.Views;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.OutputViewFactories;

public sealed record Name2EeCommandViewFactory
    (MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue) : ParsedCommandViewFactoryBase<Name2EEParser>(
    MainLayout, Clipboard, CommandQueue)
{
    public override ImmutableArray<string> SupportedCommands { get; } = ImmutableArray.Create(Commands.Name2EE);
}