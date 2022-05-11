using System.Diagnostics;
using NStack;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer;

public record TopLevelViews(
    Toplevel Toplevel,
    Window Window,
    TabView TabView,
    TextField CommandInput);

public record DefaultCommandViews(
    Window Window,
    ListView OutputListView,
    TextField FilterTextField);

public static class CommandViewsExtensions
{
    public static DefaultCommandViews SetupLogic<T>(this DefaultCommandViews @this, IClipboard clipboard, T[] initialSource)
        where T : IOutputLine
    {
        var lastFilter = "";

        @this.OutputListView.SetSource(initialSource);

        @this.OutputListView.KeyPress += args =>
        {
            switch (args.KeyEvent.Key)
            {
                case Key.CtrlMask | Key.C:
                    clipboard.SetClipboardData(@this.OutputListView.Source.ToList()[@this.OutputListView.SelectedItem]?.ToString());
                    args.Handled = true;
                    break;
            }
        };

        var commandHistory = new HistoryList<string>();

        void ProcessEnterKey()
        {
            var filter = (@this.FilterTextField.Text ?? ustring.Empty).ToString()!;

            if (lastFilter.Equals(filter)) return;

            if (string.IsNullOrEmpty(filter))
            {
                @this.OutputListView.SetSource(initialSource);
            }
            else
            {
                var filteredOutput =
                    initialSource
                        .Where(line => line.Line.Contains(filter, StringComparison.OrdinalIgnoreCase))
                        .ToArray();
                @this.OutputListView.SetSource(filteredOutput);
            }

            lastFilter = filter;
            if (!string.IsNullOrWhiteSpace(filter))
                commandHistory.AddCommand(filter);
        }

        @this.FilterTextField
            .AddClipboard(clipboard)
            .AddCommandHistory(commandHistory)
            .KeyPress += args =>
        {
            if (args.KeyEvent.Key == Key.Enter)
            {
                ProcessEnterKey();
                args.Handled = true;
            }
        };

        return @this;
    }
}

public static class ViewsExtensions
{
    public static TopLevelViews SetupLogic(this TopLevelViews @this, CommandQueue queue, IClipboard clipboard, HistoryList<string> commandHistory)
    {
        void ProcessEnterKey()
        {
            var command = (@this.CommandInput.Text ?? ustring.Empty).ToString()!;
            if (string.IsNullOrWhiteSpace(command)) return;
            queue.SendCommand(command);
        }

        @this.CommandInput
            .AddClipboard(clipboard)
            .AddCommandHistory(commandHistory)
            .KeyPress += args =>
        {
            // TODO: check if this is a bug and try to fix it
            // TODO: try to fix double finger scroll in gui.cs?
            // not sure why but enter key press on filter text filed triggers this one too. A bug?
            if (!@this.CommandInput.HasFocus) return;

            if (args.KeyEvent.Key == Key.Enter)
            {
                ProcessEnterKey();
                args.Handled = true;
            }
        };

        return @this;
    }
}

public class UI
{
    public static Func<Exception, bool> MakeExceptionHandler(TabManager tabManager, IClipboard clipboard)
    {
        return exn =>
        {
            var errorSource =
                exn.ToString()
                   .Split(Environment.NewLine)
                   .Select(x => new OutputLine(x)).ToArray();

            var commandViews =
                MakeDefaultCommandViews()
                    .SetupLogic(clipboard, errorSource);

            var tab =
                new TabView.Tab("Unhandled exception", commandViews.Window);

            tabManager.SetTab(exn.Message, tab);
            return true;
        };
    }

    public static DefaultCommandViews MakeDefaultCommandViews()
    {
        var window = new Window
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var listView = new ListView
        {
            Height = Dim.Fill() - Dim.Sized(3),
            Width = Dim.Fill()
        };

        var filterFrame = new FrameView("Filter")
        {
            Y = Pos.Bottom(listView),
            Height = 3,
            Width = Dim.Fill()
        };

        var filterField = new TextField
        {
            Height = 1,
            Width = Dim.Fill()
        };

        window.With(
            listView,
            filterFrame.With(
                filterField));

        return new(window, listView, filterField);
    }

    public static TopLevelViews MakeViews(Toplevel toplevel)
    {
        var window = new Window
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var tabView = new TabView
        {
            Width = Dim.Fill(),
            Height = Dim.Fill() - Dim.Sized(3),
        };

        var commandFrame = new FrameView("Command")
        {
            Y = Pos.Bottom(tabView),
            Height = 3,
            Width = Dim.Fill()
        };

        var commandInput = new TextField
        {
            Width = Dim.Fill(),
            Height = 1
        };

        toplevel.With(
            window.With(
                tabView,
                commandFrame.With(
                    commandInput)));

        return new(toplevel, window, tabView, commandInput);
    }
}