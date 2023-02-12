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

public interface IThreadState
{
    ReadOnlyMemory<char> ThreadState { get; }
}

public interface ISyncBlockOwnerAddress
{
    ReadOnlyMemory<char> SyncBlockOwnerAddress { get; }
}

public static class SubcommandsView
{
    private class ButtonFactory
    {
#pragma warning disable CA1069
        private enum Priority
        {
            Copy = 0,
            DumpHeap = 1,
            GcRoot = 2,
            DumpObjects = 3,
            DumpMethodTable = 4,
            PStacks = 5,
            SetThread = 5,

            // let them have the lowest priority for now because there are lot of them
            DumpMemory = 10
        }
#pragma warning restore CA1069

        
        private readonly Dialog _dialog = new ("Available commands");
        private readonly Func<OutputLine, IEnumerable<(Priority priority, Button button)>>[] _buttonFactories;
        private int _yAxis;

        public ButtonFactory(
            TopLevelViews topLevelViews,
            IClipboard clipboard,
            CommandQueue commandQueue)
        {
            CommandQueue = commandQueue ?? throw new ArgumentNullException(nameof(commandQueue));
            Clipboard = clipboard ?? throw new ArgumentNullException(nameof(clipboard));
            TopLevelViews = topLevelViews ?? throw new ArgumentNullException(nameof(topLevelViews));
            
            _buttonFactories = new[]
            {
                GetAddressButtons,
                GetMethodTableButtons,
                GetTypeNameButtons,
                GetClrThreadButtons,
                GetOsThreadIdButtons,
            };
        }

        private TopLevelViews TopLevelViews { get; }

        private IClipboard Clipboard { get; }

        private CommandQueue CommandQueue { get; }

        public Dialog? TryGetSubcommandsDialog(OutputLine line)
        {
            _ = line ?? throw new ArgumentNullException(nameof(line));

            var buttons = _buttonFactories.SelectMany(factory => factory(line)).ToArray();

            if (buttons.Length is 0)
                return null;

            var orderedButtons = buttons
                .OrderBy(x => x.priority)
                .Select(x => x.button)
                .ToArray();

            // 6 and 2 are pop-up dialog borders
            int width = orderedButtons.MaxBy(values => values.Text.Length)!.Text.Length + 6;
            int height = orderedButtons.Length + 2;

            foreach (var button in orderedButtons)
                _dialog.AddButton(button);

            _dialog.Width = width;
            _dialog.Height = height;

            return _dialog;
        }
        
        private IEnumerable<(Priority, Button)> GetAddressButtons(OutputLine line)
        {
            if (line is not IAddress address)
                yield break;
            
            var data = address.Address.ToString();
            yield return (
                Priority.Copy,
                MakeButton("Copy address", () => Clipboard.SetClipboardData(data), MakePasteAction(data)));
            yield return (
                Priority.GcRoot,
                MakeCommandButton("Find GC root", $"{Commands.GcRoot} {data}"));

            yield return (Priority.DumpObjects,
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
                            const string threadAnchor = "~~~~ ";
                            if (x.IndexOf(threadAnchor, StringComparison.Ordinal) is not -1 and var index)
                            {
                                var values = x[(index + threadAnchor.Length)..]
                                    .Split(",", StringSplitOptions.TrimEntries);
                                return values.AsSpan().Contains(normalizedOsId);
                            }

                            return false;
                        });
                        return views;
                    })
                );
        }
        
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

        private Action MakePasteAction(string data) =>
            () =>
            {
                TopLevelViews.CommandInput.Paste(data);
                TopLevelViews.CommandInput.SetFocus();
            };

        private Button MakeButton(string title, Action onClick, Action onTab)
        {
            var button = new Button(0, _yAxis++, title);

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