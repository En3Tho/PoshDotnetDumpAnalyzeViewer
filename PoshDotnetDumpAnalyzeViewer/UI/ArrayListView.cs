﻿using System.Collections.ObjectModel;
using Terminal.Gui;

namespace PoshDotnetDumpAnalyzeViewer.UI;

public class ArrayListView<T> : ListView
{
    public ArrayListView(T[] initialSource)
    {
        InitialSource = initialSource;
        SetSource(initialSource);
    }

    public T[] InitialSource { get; }

    public void SetSource(T[] source) => base.SetSource(new ObservableCollection<T>(source));

    public new T[] Source => (T[]) base.Source.ToList();
}