namespace PoshDotnetDumpAnalyzeViewer.Utilities;

public class HistoryList<T>
{
    private readonly List<T> _items = [];
    private int _currentIndex;

    private void Remove(T item)
    {
        if (_items.IndexOf(item) is >= 0 and var idx)
            _items.RemoveAt(idx);
    }

    private T? GetCurrent()
    {
        if (_currentIndex >= 0 && _currentIndex < _items.Count)
            return _items[_currentIndex];
        return default;
    }

    public void Add(T command)
    {
        Remove(command);

        _items.Add(command);
        _currentIndex = _items.Count;
    }

    public T? Previous()
    {
        _currentIndex = Math.Max(0, _currentIndex - 1);
        return GetCurrent();
    }

    public T? Next()
    {
        _currentIndex = Math.Min(_currentIndex + 1, _items.Count);
        return GetCurrent();
    }
}