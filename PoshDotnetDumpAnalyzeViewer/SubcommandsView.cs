using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public interface IOutputLine<T>
{
    string Line { get; }
}

public interface IThreadId
{
    Memory<char> TheadId { get; }
}

public interface IAddress
{
    Memory<char> Address { get; }
}

public interface IMethodTable
{
    Memory<char> MethodTable { get; }
}

public interface ITypeName
{
    Memory<char> TypeName { get; }
}

public interface IHelpCommand
{
    string[] Commands { get; }
}

public static class SubcommandsView
{
    public static Dialog ForOutputLine<T>(IOutputLine<T> line, IClipboard clipboard, CommandQueue queue)
    {
        // width is max commands width + padding
        // height is commands count + padding

        var dialog = new Dialog("Available commands"); // to popup

        Button MakeButton(string title, Action action)
        {
            var button = new Button(title);
            button.Clicked += () =>
            {
                Application.RequestStop(dialog);
                action();
            };
            return button;
        }

        var buttons = new List<(int, Button)>();

        if (line is IAddress address)
        {
            var data = address.Address.ToString();
            buttons.Add((1, MakeButton("Copy address", () => clipboard.SetClipboardData(data))));
        }

        if (line is IMethodTable methodTable)
        {
            var data = methodTable.MethodTable.ToString();
            buttons.Add((1, MakeButton("Copy method table", () => clipboard.SetClipboardData(data))));
        }

        if (line is ITypeName typeName)
        {
            var data = typeName.TypeName.ToString();
            buttons.Add((1, MakeButton("Copy type name", () => clipboard.SetClipboardData(data))));
        }

        if (line is IThreadId threadId)
        {
            var data = threadId.TheadId.ToString();
            buttons.Add((1, MakeButton("Copy thread id", () => clipboard.SetClipboardData(data))));
        }

        if (buttons.Count > 0)
        {
            var width = buttons.MaxBy(values => values.Item2.Text).Item2.Text.Length;
            var height = buttons.Count;

            dialog.Width = width;
            dialog.Height = height;
        }

        return dialog;
    }
}