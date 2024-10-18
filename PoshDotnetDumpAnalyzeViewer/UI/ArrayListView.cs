using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.UI;

// v2 notes: new ObservableCollection(source)

public class ArrayListView<T> : ListView
{
    public ArrayListView(T[] initialSource)
    {
        InitialSource = initialSource;
        SetSource(initialSource);
    }

    public T[] InitialSource { get; }

    public void SetSource(T[] source) => base.SetSource(source);

    public new T[] Source => (T[]) base.Source.ToList();
}