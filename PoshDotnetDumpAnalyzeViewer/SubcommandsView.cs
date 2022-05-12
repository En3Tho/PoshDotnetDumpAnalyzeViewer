using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public interface IOutputLine
{
    string Line { get; }
}

public interface IThreadId
{
    ReadOnlyMemory<char> TheadId { get; }
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

public static class SubcommandsView
{
    private const int CopyPriority = 0;
    private const int DumpHeapPriority = 1;
    private const int GcRootPriority = 2;
    private const int DumpObjectsPriority = 3;
    private const int DumpMethodTablePriority = 4;
    private const int DumpMemoryPriority = 10; // let them have the lowest priority for now because there are lot of them

    public static Dialog? TryGetSubcommandsDialog(OutputLine line, IClipboard clipboard, CommandQueue queue)
    {
        var dialog = new Dialog("Available commands");
        var yAxis = 0;

        Button MakeButton(string title, Action onClick)
        {
            var button = new Button(0, yAxis++, title);

            button.KeyPress += args =>
            {
                if (args.KeyEvent.Key == Key.Tab)
                {
                    // add text to command view's text
                    //
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

        Button MakeCommandButton(string title, string command) =>
            MakeButton(title, () => queue.SendCommand(command));

        var buttonsWithPriorities = new List<(int Priority, Func<Button> Button)>();

        if (line is IAddress address)
        {
            var data = address.Address.ToString();
            buttonsWithPriorities.Add((CopyPriority, () => MakeButton("Copy address", () => clipboard.SetClipboardData(data))));
            buttonsWithPriorities.Add((GcRootPriority, () => MakeCommandButton("Find GC root", $"{Commands.GcRoot} {data}")));

            buttonsWithPriorities.Add((DumpObjectsPriority, () => MakeCommandButton("Dump object", $"{Commands.DumpObject} {data}")));

            buttonsWithPriorities.Add((DumpMemoryPriority, () => MakeCommandButton("Dump memory", $"{Commands.DumpMemory} {data}")));
            buttonsWithPriorities.Add((DumpMemoryPriority, () => MakeCommandButton("Dump memory as bytes", $"{Commands.DumpMemoryAsBytes} {data}")));
            buttonsWithPriorities.Add((DumpMemoryPriority, () => MakeCommandButton("Dump memory as chars", $"{Commands.DumpMemoryAsChars} {data}")));
            buttonsWithPriorities.Add((DumpMemoryPriority, () => MakeCommandButton("Dump memory as byte string", $"{Commands.DumpMemoryAsByteString} {data}")));
            buttonsWithPriorities.Add((DumpMemoryPriority, () => MakeCommandButton("Dump memory as char string", $"{Commands.DumpMemoryAsCharString} {data}")));
            buttonsWithPriorities.Add((DumpMemoryPriority, () => MakeCommandButton("Dump memory as native ints", $"{Commands.DumpMemoryAsPointers} {data}")));
            buttonsWithPriorities.Add((DumpMemoryPriority, () => MakeCommandButton("Dump memory as shorts", $"{Commands.DumpMemoryAsWords} {data}")));
            buttonsWithPriorities.Add((DumpMemoryPriority, () => MakeCommandButton("Dump memory as int", $"{Commands.DumpMemoryAsDoubleWords} {data}")));
            buttonsWithPriorities.Add((DumpMemoryPriority, () => MakeCommandButton("Dump memory as longs", $"{Commands.DumpMemoryAsQuadWords} {data}")));
        }

        if (line is IMethodTable methodTable)
        {
            var data = methodTable.MethodTable.ToString();
            buttonsWithPriorities.Add((CopyPriority, () => MakeButton("Copy method table", () => clipboard.SetClipboardData(data))));
            buttonsWithPriorities.Add((DumpHeapPriority, () => MakeCommandButton("Dump heap (method table)", $"{Commands.DumpHeap} -mt {data}")));
            buttonsWithPriorities.Add((DumpMethodTablePriority, () => MakeCommandButton("Dump method table", $"{Commands.DumpMethodTable} {data}")));
        }

        if (line is ITypeName typeName)
        {
            var data = typeName.TypeName.ToString();
            buttonsWithPriorities.Add((CopyPriority, () => MakeButton("Copy type name", () => clipboard.SetClipboardData(data))));
            buttonsWithPriorities.Add((DumpHeapPriority, () => MakeCommandButton("Dump heap (type)", $"{Commands.DumpHeap} -type {data}")));
        }

        if (line is IThreadId threadId)
        {
            var data = threadId.TheadId.ToString();
            buttonsWithPriorities.Add((CopyPriority, () => MakeButton("Copy thread id", () => clipboard.SetClipboardData(data))));
        }

        // todo: action on tab.

        if (buttonsWithPriorities.Count > 0)
        {
            var buttons = buttonsWithPriorities.OrderBy(x => x.Priority).Select(x => x.Button()).ToArray();

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