using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public static class SubcommandsView
{
#pragma warning disable CA1069
    private enum Priority
    {
        Copy = 0,
        DumpHeap = 1,
        PrintException = 1,
        GcRoot = 2,
        DumpObjects = 3,
        DumpMethodTable = 4,
        PStacks = 5,
        SetThread = 5,
        ThreadState = 9,
        SyncBlock = 9,

        // let them have the lowest priority for now because there are lot of them
        DumpMemory = 100
    }
#pragma warning restore CA1069

    private record ButtonFactory(TopLevelViews TopLevelViews, IClipboard Clipboard, CommandQueue CommandQueue)
    {
        private readonly Dialog _dialog = new("Available commands");

        private Button MakeButton(string title, Action onClick, Action onTab)
        {
            var button = new Button(0, 0, title);

            button.KeyPress += args =>
            {
                if (args.KeyEvent.Key == Key.Tab)
                {
                    Application.RequestStop(_dialog);
                    onTab();
                    args.Handled = true;
                }
            };

            button.Clicked += () =>
            {
                Application.RequestStop(_dialog);
                onClick();
            };

            return button;
        }

        private Action MakePasteAction(string data) =>
            () =>
            {
                TopLevelViews.CommandInput.Paste(data);
                TopLevelViews.CommandInput.SetFocus();
            };

        private Button MakeCommandButton(
            string title,
            string command,
            bool ignoreOutput = false,
            bool forceRefresh = false,
            Func<CommandOutputViews, CommandOutputViews>? customAction = null)
        {
            return MakeButton(
                title,
                () => CommandQueue.SendCommand(command, forceRefresh, ignoreOutput, customAction),
                MakePasteAction(command));
        }

        private IEnumerable<(Priority, Button)> GetObjectAddressButtons(OutputLine line)
        {
            if (line is not IObjectAddress address)
                yield break;

            var data = address.Address.ToString();
            yield return (
                Priority.Copy,
                MakeButton("Copy address", () => Clipboard.SetClipboardData(data), MakePasteAction(data)));
            yield return (
                Priority.GcRoot,
                MakeCommandButton("Find GC root", $"{Commands.GCRoot} {data}"));
            yield return (
                Priority.DumpObjects,
                MakeCommandButton("Calculate object size", $"{Commands.ObjSize} {data}"));
            yield return (
                Priority.DumpObjects,
                MakeCommandButton("Dump object", $"{Commands.DumpObject} {data}"));
            yield return (
                Priority.DumpMemory,
                MakeCommandButton("Dump memory", $"{Commands.DumpMemory} {data}"));
            yield return (
                Priority.DumpMemory,
                MakeCommandButton("Dump memory as bytes", $"{Commands.DumpMemoryAsBytes} {data}"));
            yield return (
                Priority.DumpMemory,
                MakeCommandButton("Dump memory as chars", $"{Commands.DumpMemoryAsChars} {data}"));
            yield return (
                Priority.DumpMemory,
                MakeCommandButton("Dump memory as byte string", $"{Commands.DumpMemoryAsByteString} {data}"));
            yield return (
                Priority.DumpMemory,
                MakeCommandButton("Dump memory as char string", $"{Commands.DumpMemoryAsCharString} {data}"));
            yield return (
                Priority.DumpMemory,
                MakeCommandButton("Dump memory as native ints", $"{Commands.DumpMemoryAsPointers} {data}"));
            yield return (
                Priority.DumpMemory,
                MakeCommandButton("Dump memory as shorts", $"{Commands.DumpMemoryAsWords} {data}"));
            yield return (
                Priority.DumpMemory,
                MakeCommandButton("Dump memory as int", $"{Commands.DumpMemoryAsDoubleWords} {data}"));
            yield return (
                Priority.DumpMemory,
                MakeCommandButton("Dump memory as longs", $"{Commands.DumpMemoryAsQuadWords} {data}"));
        }

        private IEnumerable<(Priority, Button)> GetExceptionObjectAddressButtons(OutputLine line)
        {
            if (line is not IExceptionObjectAddress address)
                yield break;

            var data = address.Address.ToString();
            yield return (
                Priority.PrintException,
                MakeCommandButton("Print Exception", $"{Commands.PrintException} {data}"));
        }

        private IEnumerable<(Priority, Button)> GetSyncBlockOwnerAddressButtons(OutputLine line)
        {
            if (line is not ISyncBlockOwnerAddress threadState)
                yield break;

            var data = threadState.SyncBlockOwnerAddress.ToString();
            yield return (
                Priority.Copy,
                MakeButton("Copy syncblock owner address", () => Clipboard.SetClipboardData(data), MakePasteAction(data)));
            yield return (
                Priority.DumpObjects,
                MakeCommandButton("Dump syncblock owner", $"{Commands.DumpObject} {data}"));
        }

        private IEnumerable<(Priority, Button)> GetSyncBlockAddressButtons(OutputLine line)
        {
            if (line is not ISyncBlockAddress syncBlockAddress)
                yield break;

            var data = syncBlockAddress.SyncBlockAddress.ToString();
            yield return (
                Priority.Copy,
                MakeButton("Copy syncblock address", () => Clipboard.SetClipboardData(data), MakePasteAction(data)));
            yield return (
                Priority.DumpObjects,
                MakeCommandButton("Dump syncblock", $"{Commands.DumpMemory} {data}"));
        }

        private IEnumerable<(Priority, Button)> GetEEClassAddressButtons(OutputLine line)
        {
            if (line is not IEEClassAddress eeClassAddress)
                yield break;

            var data = eeClassAddress.EEClassAddress.ToString();
            yield return (
                Priority.Copy,
                MakeButton("Copy EEClass address", () => Clipboard.SetClipboardData(data), MakePasteAction(data)));
            yield return (
                Priority.DumpObjects,
                MakeCommandButton("Dump EEClass", $"{Commands.DumpClass} {data}"));
        }
        
        private IEnumerable<(Priority, Button)> GetDomainAddressButtons(OutputLine line)
        {
            if (line is not IDomainAddress domainAddress)
                yield break;

            var data = domainAddress.DomainAddress.ToString();
            yield return (
                Priority.Copy,
                MakeButton("Copy Domain address", () => Clipboard.SetClipboardData(data), MakePasteAction(data)));
            yield return (
                Priority.DumpObjects,
                MakeCommandButton("Dump Domain", $"{Commands.DumpDomain} {data}"));
        }
        
        private IEnumerable<(Priority, Button)> GetAssemblyAddressButtons(OutputLine line)
        {
            if (line is not IAssemblyAddress assemblyAddress)
                yield break;

            var data = assemblyAddress.AssemblyAddress.ToString();
            yield return (
                Priority.Copy,
                MakeButton("Copy Assembly address", () => Clipboard.SetClipboardData(data), MakePasteAction(data)));
            yield return (
                Priority.DumpObjects,
                MakeCommandButton("Dump Assembly", $"{Commands.DumpAssembly} {data}"));
        }
        
        private IEnumerable<(Priority, Button)> GetModuleAddressButtons(OutputLine line)
        {
            if (line is not IModuleAddress moduleAddress)
                yield break;

            var data = moduleAddress.ModuleAddress.ToString();
            yield return (
                Priority.Copy,
                MakeButton("Copy Module address", () => Clipboard.SetClipboardData(data), MakePasteAction(data)));
            yield return (
                Priority.DumpObjects,
                MakeCommandButton("Dump Module", $"{Commands.DumpModule} {data}"));
        }
        
        private IEnumerable<(Priority, Button)> GetMethodTableButtons(OutputLine line)
        {
            if (line is not IMethodTable methodTable)
                yield break;
            var data = methodTable.MethodTable.ToString();
            yield return (
                Priority.Copy,
                MakeButton("Copy method table", () => Clipboard.SetClipboardData(data), MakePasteAction(data)));
            yield return (
                Priority.DumpHeap,
                MakeCommandButton("Dump heap (method table)", $"{Commands.DumpHeap} -mt {data}"));
            yield return (
                Priority.DumpMethodTable,
                MakeCommandButton("Dump method table", $"{Commands.DumpMethodTable} {data}"));
        }

        private IEnumerable<(Priority, Button)> GetTypeNameButtons(OutputLine line)
        {
            if (line is not ITypeName typeName)
                yield break;

            var data = typeName.TypeName.ToString();
            yield return (
                Priority.Copy,
                MakeButton("Copy type name", () => Clipboard.SetClipboardData(data), MakePasteAction(data)));
            yield return (
                Priority.DumpHeap,
                MakeCommandButton("Dump heap (type)", $"{Commands.DumpHeap} -type {data}"));
        }

        private IEnumerable<(Priority, Button)> GetSyncBlockOwnerTypeNameButtons(OutputLine line)
        {
            if (line is not ISyncBlockOwnerTypeName typeName)
                yield break;

            var data = typeName.SyncBlockOwnerTypeName.ToString();
            yield return (
                Priority.Copy,
                MakeButton("Copy sync block owner type name", () => Clipboard.SetClipboardData(data), MakePasteAction(data)));
            yield return (
                Priority.DumpHeap,
                MakeCommandButton("Dump heap (sync block owner type)", $"{Commands.DumpHeap} -type {data}"));
        }

        private IEnumerable<(Priority, Button)> GetSyncBlockIndexButtons(OutputLine line)
        {
            if (line is not ISyncBlockIndex syncBlockIndex)
                yield break;

            var data = syncBlockIndex.SyncBlockIndex.ToString();
            yield return (
                Priority.Copy,
                MakeButton("Copy sync block index", () => Clipboard.SetClipboardData(data), MakePasteAction(data)));

            yield return (
                Priority.SyncBlock,
                MakeCommandButton("Show syncblock info", $"{Commands.SyncBlock} {data}"));
        }

        private IEnumerable<(Priority, Button)> GetClrThreadButtons(OutputLine line)
        {
            // TODO: not sure if clr thread id is any useful
            // There can exist a mapping between clr thread id and os thread id. Can use it behind the scenes
            if (line is not IClrThreadId clrThreadId)
                yield break;

            var data = clrThreadId.ClrThreadId.ToString();
            yield return (
                Priority.Copy,
                MakeButton("Copy CLR thread id", () => Clipboard.SetClipboardData(data), MakePasteAction(data)));
        }

        private IEnumerable<(Priority, Button)> GetOsThreadIdButtons(OutputLine line)
        {
            // Not all OSThreadIds are linked with CLR
            // It might be useful to filter out native-only ones
            if (line is not IOsThreadId osThreadId)
                yield break;

            var data = osThreadId.OsThreadId.ToString();
            data = data.PadLeft(data.Length + data.Length % 4, '0');
            var idAsInt = Convert.ToInt32(data, 16);

            yield return (
                Priority.Copy,
                MakeButton("Copy OS thread id", () => Clipboard.SetClipboardData(data), MakePasteAction(data)));

            yield return (
                Priority.SetThread,
                MakeCommandButton("Set as current thread", $"{Commands.SetThread} -t {idAsInt}", ignoreOutput: true));

            yield return (
                Priority.SetThread,
                MakeCommandButton("Set as current thread and display call stack",
                    $"{Commands.SetThread} -t {idAsInt}",
                    ignoreOutput: true,
                    customAction: views =>
                    {
                        CommandQueue.SendCommand($"{Commands.ClrStack}", forceRefresh: true);
                        return views;
                    }));

            yield return (
                Priority.SetThread,
                MakeCommandButton("Set as current thread and display call stack (full info)",
                    $"{Commands.SetThread} -t {idAsInt}",
                    ignoreOutput: true,
                    customAction: views =>
                    {
                        CommandQueue.SendCommand($"{Commands.ClrStack} -a", forceRefresh: true);
                        return views;
                    }));

            yield return (
                Priority.PStacks,
                MakeCommandButton("Find thread in parallel stacks",
                    $"{Commands.ParallelStacks} -a",
                    customAction: views =>
                    {
                        var normalizedOsId = Convert.ToString(idAsInt, 16);
                        views.FilterTextField.Text = normalizedOsId;
                        views.OutputListView.TryFindItemAndSetSelected(x =>
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
                        return views;
                    })
            );
        }

        private IEnumerable<(Priority, Button)> GetThreadsStateButtons(OutputLine line)
        {
            if (line is not IThreadState threadState)
                yield break;

            var data = threadState.ThreadState.ToString();
            yield return (
                Priority.ThreadState,
                MakeCommandButton("Pretty print thread state", $"{Commands.ThreadState} {data}"));
        }

        private IEnumerable<(Priority, Button)> GetParallelStacksButtons(OutputLine line)
        {
            if (line is not ParallelStacksOutputLine)
                yield break;

            yield return (
                Priority.PStacks,
                MakeCommandButton("Shrink call stacks",
                    $"{Commands.ParallelStacks} -a",
                    forceRefresh: true,
                    customAction: views =>
                    {
                        var source = views.OutputListView.Source;
                        var shrinkedSource = ParallelStacksOutputFactory.ShrinkParallelStacksOutput(source);
                        views.OutputListView.SetSource(shrinkedSource);
                        return views;
                    })
            );

            yield return (
                Priority.PStacks,
                MakeCommandButton("Restore call stacks",
                    $"{Commands.ParallelStacks} -a",
                    forceRefresh: true)
            );
        }

        private Func<OutputLine, IEnumerable<(Priority priority, Button button)>>[] GetFactories()
        {
            return new[]
            {
                GetObjectAddressButtons,
                GetExceptionObjectAddressButtons,
                GetSyncBlockAddressButtons,
                GetSyncBlockOwnerAddressButtons,
                GetMethodTableButtons,
                GetTypeNameButtons,
                GetSyncBlockOwnerTypeNameButtons,
                GetSyncBlockIndexButtons,
                GetClrThreadButtons,
                GetOsThreadIdButtons,
                GetThreadsStateButtons,
                GetEEClassAddressButtons,
                GetModuleAddressButtons,
                GetAssemblyAddressButtons,
                GetDomainAddressButtons,
                GetParallelStacksButtons
            };
        }

        public Dialog? TryGetSubcommandsDialog(OutputLine line)
        {
            var buttons =
                GetFactories()
                    .SelectMany(factory => factory(line))
                    .OrderBy(x => x.priority)
                    .Select(x => x.button)
                    .ToArray();

            if (buttons.Length is 0)
                return null;

            // 6 and 2 are pop-up dialog borders
            var width = buttons.MaxBy(values => values.Text.Length)!.Text.Length + 6;
            var height = buttons.Length + 2;

            for (var i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                button.Y = i;
                _dialog.AddButton(button);
            }

            _dialog.Width = width;
            _dialog.Height = height;

            return _dialog;
        }
    }

    public static Dialog? TryGetSubcommandsDialog(
        TopLevelViews topLevelViews,
        OutputLine line,
        IClipboard clipboard,
        CommandQueue commandQueue)
    {
        var factory = new ButtonFactory(topLevelViews, clipboard, commandQueue);
        return factory.TryGetSubcommandsDialog(line);
    }
}