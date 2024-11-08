namespace PoshDotnetDumpAnalyzeViewerModule;

public sealed class CurrentDirectoryScope(string path) : IDisposable
{
    private static string SwitchCurrentDirectory(string path)
    {
        var currentPath = Environment.CurrentDirectory;
        Environment.CurrentDirectory = path;
        return currentPath;
    }

    private readonly string _pathToRestore = SwitchCurrentDirectory(path);

    public void Dispose()
    {
        Environment.CurrentDirectory = _pathToRestore;
    }
}