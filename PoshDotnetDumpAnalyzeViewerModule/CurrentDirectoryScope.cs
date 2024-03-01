namespace PoshDotnetDumpAnalyzeViewerModule;

public sealed record CurrentDirectoryScope(string Path) : IDisposable
{
    private static string SwitchCurrentDirectory(string path)
    {
        var currentPath = Environment.CurrentDirectory;
        Environment.CurrentDirectory = path;
        return currentPath;
    }

    private readonly string _pathToRestore = SwitchCurrentDirectory(Path);

    public void Dispose()
    {
        Environment.CurrentDirectory = _pathToRestore;
    }
}