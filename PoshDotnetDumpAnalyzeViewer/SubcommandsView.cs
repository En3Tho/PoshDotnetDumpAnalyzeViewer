using System.Diagnostics.CodeAnalysis;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public interface IOutputLine<T>
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
    public const int CopyPriority = 0;
    public const int DumpHeapPriority = 1;
    public const int DumpMethodTablePriority = 1;
    public const int GcRootPriority = 2;

    public static bool TryGetSubcommandsDialog(OutputLine line, IClipboard clipboard, CommandQueue queue, [NotNullWhen(true)] out Dialog? dialog)
    {
        var result = new Dialog("Available commands");
        var yAxis = 0;

        Button MakeButton(string title, Action onClick)
        {
            var button = new Button(0, yAxis++, title);
            button.Clicked += () =>
            {
                Application.RequestStop(result);
                onClick();
            };

            return button;
        }

        Button MakeCommandButton(string command) => MakeButton(command, () => queue.SendCommand(command));

        var buttonsWithPriorities = new List<(int Priority, Func<Button> Button)>();

        if (line is IAddress address)
        {
            var data = address.Address.ToString();
            buttonsWithPriorities.Add((CopyPriority, () => MakeButton("Copy address", () => clipboard.SetClipboardData(data))));
            buttonsWithPriorities.Add((GcRootPriority, () => MakeCommandButton($"{Commands.GcRoot} {data}")));
        }

        if (line is IMethodTable methodTable)
        {
            var data = methodTable.MethodTable.ToString();
            buttonsWithPriorities.Add((CopyPriority, () => MakeButton("Copy method table", () => clipboard.SetClipboardData(data))));
            buttonsWithPriorities.Add((DumpHeapPriority, () => MakeCommandButton($"{Commands.DumpHeap} -mt {data}")));
            buttonsWithPriorities.Add((DumpMethodTablePriority, () => MakeCommandButton($"{Commands.DumpMethodTable} {data}")));
        }

        if (line is ITypeName typeName)
        {
            var data = typeName.TypeName.ToString();
            buttonsWithPriorities.Add((CopyPriority, () => MakeButton("Copy type name", () => clipboard.SetClipboardData(data))));
            buttonsWithPriorities.Add((DumpHeapPriority, () => MakeCommandButton($"{Commands.DumpHeap} -type {data}")));
        }

        if (line is IThreadId threadId)
        {
            var data = threadId.TheadId.ToString();
            buttonsWithPriorities.Add((CopyPriority, () => MakeButton("Copy thread id", () => clipboard.SetClipboardData(data))));
        }

        if (buttonsWithPriorities.Count > 0)
        {
            var buttons = buttonsWithPriorities.OrderBy(x => x.Priority).Select(x => x.Button()).ToArray();

            var width = buttons.MaxBy(values => values.Text)!.Text.Length + 6;
            var height = buttons.Length + 2;

            foreach (var button in buttons)
                result.AddButton(button);

            result.Width = width;
            result.Height = height;

            dialog = result;
            return true;
        }

        dialog = null;
        return false;
    }
}