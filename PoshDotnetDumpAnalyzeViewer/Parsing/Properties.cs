namespace PoshDotnetDumpAnalyzeViewer.Parsing;

public static class Properties
{
    public const string ObjectAddress = "ObjectAddress";
    public const string ExceptionObjectAddress = "ClrThreadId";
    public const string SyncBlockAddress = "SyncBlockAddress";
    public const string EEClassAddress = "EEClassAddress";
    public const string AssemblyAddress = "AssemblyAddress";
    public const string ModuleAddress = "ModuleAddress";
    public const string MethodTable = "MethodTable";
    public const string TypeName = "TypeName";
    public const string SyncBlockOwnerTypeName = "SyncBlockOwnerTypeName";
    public const string SyncBlockOwnerAddress = "SyncBlockOwnerAddress";
    public const string SyncBlockIndex = "SyncBlockIndex";
    public const string ClrThreadId = "ClrThreadId";
    public const string OsThreadId = "OsThreadId";
    public const string ThreadState = "ThreadState";
}

public static class SpecialProperties
{
    public const string HelpCommands = "HelpCommands";
    public const string ParallelStacksThreadNames = "ParallelStacksThreadNames";
}