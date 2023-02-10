using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public interface IOutputLine
{
    string Line { get; }
}

public interface IClrThreadId
{
    ReadOnlyMemory<char> ClrThreadId { get; }
}

public interface IAddress
{
    ReadOnlyMemory<char> Address { get; }
}

public interface IMethodTable
{
    ReadOnlyMemory<char> MethodTable { get; }
}

public interface ITypeName
{
    ReadOnlyMemory<char> TypeName { get; }
}

public interface IHelpCommand
{
    string[] Commands { get; }
}

public interface IOsThreadId
{
    ReadOnlyMemory<char> OsThreadId { get; }
}

public static class SubcommandsView
{
    private const int CopyPriority = 0;
    private const int DumpHeapPriority = 1;
    private const int GcRootPriority = 2;
    private const int DumpObjectsPriority = 3;
    private const int DumpMethodTablePriority = 4;
    private const int PStacksPriority = 5;
    private const int SetThreadPriority = 5;

    private const int
        DumpMemoryPriority = 10; // let them have the lowest priority for now because there are lot of them

    public static Dialog? TryGetSubcommandsDialog(TopLevelViews topLevelViews, OutputLine line, IClipboard clipboard,
        CommandQueue commandQueue)
    {
        var dialog = new Dialog("Available commands");
        var yAxis = 0;

        Button MakeButton(string title, Action onClick, Action onTab)
        {
            var button = new Button(0, yAxis++, title);

            button.KeyPress += args =>
            {
                if (args.KeyEvent.Key == Key.Tab)
                {
                    Application.RequestStop(dialog);
                    onTab();
                    args.Handled = true;
                }
            };

            button.Clicked += () =>
            {
                Application.RequestStop(dialog);
                onClick();
            };

            return button;
        }

        // TODO: really, make just di based commands with funcs and helpers.
        Action MakePasteAction(string data) => () =>
        {
            topLevelViews.CommandInput.Paste(data);
            topLevelViews.CommandInput.SetFocus();
        };

        Button MakeCommandButton(string title, string command, bool ignoreOutput = false, bool forceRefresh = false,
            Func<CommandOutputViews, CommandOutputViews>? customAction = null)
        {
            return MakeButton(title, () => commandQueue.SendCommand(command, forceRefresh, ignoreOutput, customAction),
                MakePasteAction(command));
        }

        var buttonsWithPriorities = new List<(int Priority, Func<Button> Button)>();

        if (line is IAddress address)
        {
            var data = address.Address.ToString();
            buttonsWithPriorities.Add((CopyPriority,
                () => MakeButton("Copy address", () => clipboard.SetClipboardData(data), MakePasteAction(data))));
            buttonsWithPriorities.Add((GcRootPriority,
                () => MakeCommandButton("Find GC root", $"{Commands.GcRoot} {data}")));

            buttonsWithPriorities.Add((DumpObjectsPriority,
                () => MakeCommandButton("Dump object", $"{Commands.DumpObject} {data}")));

            buttonsWithPriorities.Add((DumpMemoryPriority,
                () => MakeCommandButton("Dump memory", $"{Commands.DumpMemory} {data}")));
            buttonsWithPriorities.Add((DumpMemoryPriority,
                () => MakeCommandButton("Dump memory as bytes", $"{Commands.DumpMemoryAsBytes} {data}")));
            buttonsWithPriorities.Add((DumpMemoryPriority,
                () => MakeCommandButton("Dump memory as chars", $"{Commands.DumpMemoryAsChars} {data}")));
            buttonsWithPriorities.Add((DumpMemoryPriority,
                () => MakeCommandButton("Dump memory as byte string", $"{Commands.DumpMemoryAsByteString} {data}")));
            buttonsWithPriorities.Add((DumpMemoryPriority,
                () => MakeCommandButton("Dump memory as char string", $"{Commands.DumpMemoryAsCharString} {data}")));
            buttonsWithPriorities.Add((DumpMemoryPriority,
                () => MakeCommandButton("Dump memory as native ints", $"{Commands.DumpMemoryAsPointers} {data}")));
            buttonsWithPriorities.Add((DumpMemoryPriority,
                () => MakeCommandButton("Dump memory as shorts", $"{Commands.DumpMemoryAsWords} {data}")));
            buttonsWithPriorities.Add((DumpMemoryPriority,
                () => MakeCommandButton("Dump memory as int", $"{Commands.DumpMemoryAsDoubleWords} {data}")));
            buttonsWithPriorities.Add((DumpMemoryPriority,
                () => MakeCommandButton("Dump memory as longs", $"{Commands.DumpMemoryAsQuadWords} {data}")));
        }

        if (line is IMethodTable methodTable)
        {
            var data = methodTable.MethodTable.ToString();
            buttonsWithPriorities.Add((CopyPriority,
                () => MakeButton("Copy method table", () => clipboard.SetClipboardData(data), MakePasteAction(data))));
            buttonsWithPriorities.Add((DumpHeapPriority,
                () => MakeCommandButton("Dump heap (method table)", $"{Commands.DumpHeap} -mt {data}")));
            buttonsWithPriorities.Add((DumpMethodTablePriority,
                () => MakeCommandButton("Dump method table", $"{Commands.DumpMethodTable} {data}")));
        }

        if (line is ITypeName typeName)
        {
            var data = typeName.TypeName.ToString();
            buttonsWithPriorities.Add((CopyPriority,
                () => MakeButton("Copy type name", () => clipboard.SetClipboardData(data), MakePasteAction(data))));
            buttonsWithPriorities.Add((DumpHeapPriority,
                () => MakeCommandButton("Dump heap (type)", $"{Commands.DumpHeap} -type {data}")));
        }

        // TODO: not sure if clr thread id is any useful
        // There can exist a mapping between clr thread id and os thread id. Can use it behind the scenes
        if (line is IClrThreadId clrThreadId)
        {
            var data = clrThreadId.ClrThreadId.ToString();
            buttonsWithPriorities.Add((CopyPriority,
                () => MakeButton("Copy CLR thread id", () => clipboard.SetClipboardData(data), MakePasteAction(data))));
        }

        // Not all OSThreadIds are linked with CLR
        // It might be useful to filter out native-only ones
        if (line is IOsThreadId osThreadId)
        {
            var data = osThreadId.OsThreadId.ToString();
            data = data.PadLeft(data.Length + data.Length % 4, '0');
            var idAsInt = Convert.ToInt32(data, 16);

            buttonsWithPriorities.Add((CopyPriority,
                () => MakeButton("Copy OS thread id", () => clipboard.SetClipboardData(data), MakePasteAction(data))));

            buttonsWithPriorities.Add((SetThreadPriority,
                () => MakeCommandButton("Set as current thread", $"{Commands.SetThread} -t {idAsInt}",
                    ignoreOutput: true)));

            buttonsWithPriorities.Add((SetThreadPriority, () =>
                MakeCommandButton("Set as current thread and display call stack", $"{Commands.SetThread} -t {idAsInt}",
                    ignoreOutput: true, customAction: views =>
                    {
                        commandQueue.SendCommand($"{Commands.ClrStack}", forceRefresh: true);
                        return views;
                    })));

            buttonsWithPriorities.Add((SetThreadPriority, () =>
                MakeCommandButton("Set as current thread and display call stack (full info)",
                    $"{Commands.SetThread} -t {idAsInt}", ignoreOutput: true, customAction: views =>
                    {
                        commandQueue.SendCommand($"{Commands.ClrStack} -a", forceRefresh: true);
                        return views;
                    })));

            buttonsWithPriorities.Add((PStacksPriority, () => MakeCommandButton("Find thread in parallel stacks",
                    $"{Commands.ParallelStacks} -a", customAction: views =>
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
                ));
        }

        if (buttonsWithPriorities.Count > 0)
        {
            var buttons = buttonsWithPriorities.OrderBy(x => x.Priority).Select(x => x.Button()).ToArray();

            // 6 and 2 are pop-up dialog borders
            var width = buttons.MaxBy(values => values.Text.Length)!.Text.Length + 6;
            var height = buttons.Length + 2;

            foreach (var button in buttons)
                dialog.AddButton(button);

            dialog.Width = width;
            dialog.Height = height;

            return dialog;
        }

        return null;
    }
}