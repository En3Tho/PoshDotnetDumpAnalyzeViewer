using PoshDotnetDumpAnalyzeViewer.Parsing;
using PoshDotnetDumpAnalyzeViewer.UI.Behavior;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.UI.Subcommands;

public record SubcommandButtonFactory(SubcommandsView SubcommandContainer, MainLayout MainLayout, IClipboard Clipboard, CommandQueue CommandQueue)
{
    private Action MakePasteAction(string data) =>
        () =>
        {
            MainLayout.CommandInput.Paste(data);
            MainLayout.CommandInput.SetFocus();
        };

    public SubcommandButton MakeButton(SubcommandsPriority priority, string title, Action onEnter, Action? onTab = null)
    {
        return SubcommandsDialog.MakeButton(
            SubcommandContainer,
            priority,
            title,
            onEnter,
            onTab);
    }

    public SubcommandButton MakeCommandButton(
        SubcommandsPriority priority,
        string title,
        string command,
        bool ignoreOutput = false,
        bool forceRefresh = false,
        Func<View, View>? mapView = null,
        Func<string[], string[]>? mapOutput = null)
    {
        return MakeButton(
            priority,
            title,
            () => CommandQueue.SendCommand(command, forceRefresh, ignoreOutput, mapView, mapOutput),
            MakePasteAction(command));
    }

    public IEnumerable<SubcommandButton> GetSetAsCurrentThreadButtons(string osThreadId, int idAsInt)
    {
        yield return MakeCommandButton(SubcommandsPriority.SetThread,
            $"[{osThreadId}] Set as current thread", $"{Commands.SetThread} -t {idAsInt}", ignoreOutput: true);

        yield return MakeCommandButton(SubcommandsPriority.SetThread,
            $"[{osThreadId}] Set as current thread and display call stack",
            $"{Commands.SetThread} -t {idAsInt}",
            ignoreOutput: true,
            mapView: views =>
            {
                CommandQueue.SendCommand($"{Commands.ClrStack}", $"{Commands.ClrStack} ({idAsInt})", forceRefresh: true);
                return views;
            });

        yield return MakeCommandButton(SubcommandsPriority.SetThread,
            $"[{osThreadId}] Set as current thread and display call stack (full)",
            $"{Commands.SetThread} -t {idAsInt}",
            ignoreOutput: true,
            mapView: views =>
            {
                CommandQueue.SendCommand($"{Commands.ClrStack} -a", $"{Commands.ClrStack} -a ({idAsInt})", forceRefresh: true);
                return views;
            });
    }

    public IEnumerable<SubcommandButton> GetObjectAddressButtons(string address)
    {
        yield return MakeButton(SubcommandsPriority.Copy,
            "Copy address", () => Clipboard.SetClipboardData(address), MakePasteAction(address));

        yield return MakeCommandButton(SubcommandsPriority.GcRoot,
            "Find GC root", $"{Commands.GCRoot} {address}");

        yield return MakeCommandButton(SubcommandsPriority.DumpObject,
            "Calculate retained memory", $"{Commands.ObjSize} {address}");

        yield return MakeCommandButton(SubcommandsPriority.DumpObject,
            "Dump object", $"{Commands.DumpObject} {address}");

        yield return MakeCommandButton(SubcommandsPriority.DumpMemory,
            "Dump memory", $"{Commands.DumpMemory} {address}");

        yield return MakeCommandButton(SubcommandsPriority.DumpMemory,
            "Dump memory as bytes", $"{Commands.DumpMemoryAsBytes} {address}");

        yield return MakeCommandButton(SubcommandsPriority.DumpMemory,
            "Dump memory as chars", $"{Commands.DumpMemoryAsChars} {address}");

        yield return MakeCommandButton(SubcommandsPriority.DumpMemory,
            "Dump memory as byte string", $"{Commands.DumpMemoryAsByteString} {address}");

        yield return MakeCommandButton(SubcommandsPriority.DumpMemory,
            "Dump memory as char string", $"{Commands.DumpMemoryAsCharString} {address}");

        yield return MakeCommandButton(SubcommandsPriority.DumpMemory,
            "Dump memory as native ints", $"{Commands.DumpMemoryAsPointers} {address}");

        yield return MakeCommandButton(SubcommandsPriority.DumpMemory,
            "Dump memory as shorts", $"{Commands.DumpMemoryAsWords} {address}");

        yield return MakeCommandButton(SubcommandsPriority.DumpMemory,
            "Dump memory as int", $"{Commands.DumpMemoryAsDoubleWords} {address}");

        yield return MakeCommandButton(SubcommandsPriority.DumpMemory,
            "Dump memory as longs", $"{Commands.DumpMemoryAsQuadWords} {address}");
    }

    public IEnumerable<SubcommandButton> GetExceptionObjectAddressButtons(string address)
    {
        yield return MakeCommandButton(SubcommandsPriority.PrintException,
            "Print Exception", $"{Commands.PrintException} {address}");
    }

    public IEnumerable<SubcommandButton> GetSyncBlockOwnerAddressButtons(string address)
    {
        yield return MakeButton(SubcommandsPriority.Copy,
            "Copy syncblock owner address", () => Clipboard.SetClipboardData(address), MakePasteAction(address));

        yield return MakeCommandButton(SubcommandsPriority.DumpObject,
            "Dump syncblock owner", $"{Commands.DumpObject} {address}");
    }

    public IEnumerable<SubcommandButton> GetSyncBlockAddressButtons(string address)
    {
        yield return MakeButton(SubcommandsPriority.Copy,
            "Copy syncblock address", () => Clipboard.SetClipboardData(address), MakePasteAction(address));

        yield return MakeCommandButton(SubcommandsPriority.DumpObject,
            "Dump syncblock", $"{Commands.DumpMemory} {address}");
    }

    public IEnumerable<SubcommandButton> GetEEClassAddressButtons(string address)
    {
        yield return MakeButton(SubcommandsPriority.Copy,
            "Copy EEClass address", () => Clipboard.SetClipboardData(address), MakePasteAction(address));

        yield return MakeCommandButton(SubcommandsPriority.DumpObject,
            "Dump EEClass", $"{Commands.DumpClass} {address}");
    }

    public IEnumerable<SubcommandButton> GetDomainAddressButtons(string address)
    {
        yield return MakeButton(SubcommandsPriority.Copy,
            "Copy Domain address", () => Clipboard.SetClipboardData(address), MakePasteAction(address));

        yield return MakeCommandButton(SubcommandsPriority.DumpObject,
            "Dump Domain", $"{Commands.DumpDomain} {address}");
    }

    public IEnumerable<SubcommandButton> GetAssemblyAddressButtons(string address)
    {
        yield return MakeButton(SubcommandsPriority.Copy,
            "Copy Assembly address", () => Clipboard.SetClipboardData(address), MakePasteAction(address));

        yield return MakeCommandButton(SubcommandsPriority.DumpObject,
            "Dump Assembly", $"{Commands.DumpAssembly} {address}");
    }

    public IEnumerable<SubcommandButton> GetModuleAddressButtons(string address)
    {
        yield return MakeButton(SubcommandsPriority.Copy,
            "Copy Module address", () => Clipboard.SetClipboardData(address), MakePasteAction(address));

        yield return MakeCommandButton(SubcommandsPriority.DumpObject,
            "Dump Module", $"{Commands.DumpModule} {address}");
    }

    public IEnumerable<SubcommandButton> GetMethodTableButtons(string methodTable)
    {
        yield return MakeButton(SubcommandsPriority.Copy,
            "Copy method table", () => Clipboard.SetClipboardData(methodTable), MakePasteAction(methodTable));

        yield return MakeCommandButton(SubcommandsPriority.DumpObject,
            "Calculate retained memory (method table)", $"{Commands.ObjSize} -mt {methodTable}");

        yield return MakeCommandButton(SubcommandsPriority.DumpHeap,
            "Dump heap (method table)", $"{Commands.DumpHeap} -mt {methodTable}");

        yield return MakeCommandButton(SubcommandsPriority.DumpMethodTable,
            "Dump method table", $"{Commands.DumpMethodTable} {methodTable}");
    }

    public IEnumerable<SubcommandButton> GetTypeNameButtons(string typeName)
    {
        yield return MakeButton(SubcommandsPriority.Copy,
            "Copy type name", () => Clipboard.SetClipboardData(typeName), MakePasteAction(typeName));

        yield return MakeCommandButton(SubcommandsPriority.DumpHeap,
            "Dump heap (type)", $"{Commands.DumpHeap} -type {typeName}");
    }

    public IEnumerable<SubcommandButton> GetSyncBlockOwnerTypeNameButtons(string typeName)
    {
        yield return MakeButton(SubcommandsPriority.Copy,
            "Copy sync block owner type name", () => Clipboard.SetClipboardData(typeName), MakePasteAction(typeName));

        yield return MakeCommandButton( SubcommandsPriority.DumpHeap,
            "Dump heap (sync block owner type)", $"{Commands.DumpHeap} -type {typeName}");
    }

    public IEnumerable<SubcommandButton> GetSyncBlockIndexButtons(string syncBlockIndex)
    {
        yield return MakeButton(SubcommandsPriority.Copy,
            "Copy sync block index", () => Clipboard.SetClipboardData(syncBlockIndex), MakePasteAction(syncBlockIndex));

        yield return MakeCommandButton(SubcommandsPriority.SyncBlock,
            "Show syncblock info", $"{Commands.SyncBlock} {syncBlockIndex}");
    }

    public IEnumerable<SubcommandButton> GetClrThreadButtons(string clrThreadId)
    {
        // TODO: not sure if clr thread id is any useful
        // There can exist a mapping between clr thread id and os thread id. Can use it behind the scenes
        yield return MakeButton(SubcommandsPriority.Copy,
            "Copy CLR thread id", () => Clipboard.SetClipboardData(clrThreadId), MakePasteAction(clrThreadId));
    }

    public IEnumerable<SubcommandButton> GetOsThreadIdButtons(string osThreadId)
    {
        // Not all OSThreadIds are linked with CLR
        // It might be useful to filter out native-only ones
        var idAsInt = OsThreadIdReader.Read(osThreadId);

        yield return MakeButton(SubcommandsPriority.Copy,
            "Copy OS thread id", () => Clipboard.SetClipboardData(osThreadId), MakePasteAction(osThreadId));

        foreach (var button in GetSetAsCurrentThreadButtons(osThreadId, idAsInt))
            yield return button;

        yield return MakeCommandButton(SubcommandsPriority.ParallelStacks,
            "Find thread in parallel stacks",
            $"{Commands.ParallelStacks} -a -r",
            mapView: view =>
            {
                var pstacksView = (CommandOutputView) view;

                var normalizedOsId = Convert.ToString(idAsInt, 16);
                pstacksView.Filter.Text = normalizedOsId;
                pstacksView.ListView.TryFindItemAndSetSelected(x =>
                {
                    const string ThreadAnchor = "~~~~ ";
                    if (x.IndexOf(ThreadAnchor, StringComparison.Ordinal) is not -1 and var index)
                    {
                        var values = x[(index + ThreadAnchor.Length)..]
                            .Split(",", StringSplitOptions.TrimEntries);
                        return values.AsSpan().Contains(normalizedOsId);
                    }

                    return false;
                });
                return view;
            });
    }

    public IEnumerable<SubcommandButton> GetThreadsStateButtons(string threadState)
    {
        yield return MakeCommandButton(SubcommandsPriority.ThreadState,
            "Pretty print thread state", $"{Commands.ThreadState} {threadState}");
    }

    private IEnumerable<IEnumerable<SubcommandButton>> GenerateAllKnownButtons(OutputLine line)
    {
        if (line is IExceptionObjectAddress { Address: var exceptionObjectAddress })
            yield return GetExceptionObjectAddressButtons(exceptionObjectAddress);

        if (line is IObjectAddress { Address: var objectAddress })
            yield return GetObjectAddressButtons(objectAddress);

        if (line is ISyncBlockAddress { SyncBlockAddress: var syncBlockAddress })
            yield return GetSyncBlockAddressButtons(syncBlockAddress);

        if (line is ISyncBlockOwnerAddress { SyncBlockOwnerAddress: var syncBlockOwnerAddress })
            yield return GetSyncBlockOwnerAddressButtons(syncBlockOwnerAddress);

        if (line is IEEClassAddress { EEClassAddress: var eeClassAddress })
            yield return GetEEClassAddressButtons(eeClassAddress);

        if (line is IModuleAddress { ModuleAddress: var moduleAddress })
            yield return GetModuleAddressButtons(moduleAddress);

        if (line is IAssemblyAddress { AssemblyAddress: var assemblyAddress })
            yield return GetAssemblyAddressButtons(assemblyAddress);

        if (line is IDomainAddress { DomainAddress: var domainAddress })
            yield return GetDomainAddressButtons(domainAddress);

        if (line is IMethodTable { MethodTable: var methodTable })
            yield return GetMethodTableButtons(methodTable);

        if (line is ITypeName { TypeName: var typeName })
            yield return GetTypeNameButtons(typeName);

        if (line is ISyncBlockOwnerTypeName { SyncBlockOwnerTypeName: var syncBlockOwnerTypeName })
            yield return GetSyncBlockOwnerTypeNameButtons(syncBlockOwnerTypeName);

        if (line is ISyncBlockIndex { SyncBlockIndex: var syncBlockIndex })
            yield return GetSyncBlockIndexButtons(syncBlockIndex);

        if (line is IOsThreadId { OsThreadId: var osThreadId })
            yield return GetOsThreadIdButtons(osThreadId);

        if (line is IThreadState { ThreadState: var threadState })
            yield return GetThreadsStateButtons(threadState);

        if (line is IClrThreadId { ClrThreadId: var clrThreadId })
            yield return GetClrThreadButtons(clrThreadId);
    }

    public void AddFrom(OutputLine line)
    {
        var buttons =
            GenerateAllKnownButtons(line)
                .SelectMany(x => x);

        SubcommandContainer.AddButtons(buttons);
    }
}