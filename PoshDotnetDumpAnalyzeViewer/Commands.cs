using System.Text.RegularExpressions;
using PoshDotnetDumpAnalyzeViewer.Utilities;

namespace PoshDotnetDumpAnalyzeViewer;

public static partial class Commands
{
    public const string Quit = "quit";
    public const string Exit = "exit";
    public const string Q = "q";
    public const string Help = "help";
    public const string DumpHeap = "dumpheap";
    public const string DumpMethodTable = "dumpmt";
    public const string GCRoot = "gcroot";
    public const string PrintException = "printexception";

    public const string ObjSize = "objsize";
    public const string DumpExceptions = "dumpexceptions";

    public const string SetThread = "setthread";
    public const string Threads = "threads";
    public const string ClrStack = "clrstack";
    public const string ParallelStacks = "pstacks";
    public const string ClrThreads = "clrthreads";

    public const string SyncBlock = "syncblk";
    public const string ThreadState = "threadstate";

    public const string DumpObject = "do";
    public const string DumpArray = "dumparray";
    public const string DumpConcurrentDictionary = "dumparray";
    public const string DumpConcurrentQueue = "dumparray";

    public const string DumpClass = "dumpclass";
    public const string DumpModule = "dumpmodule";
    public const string DumpAssembly = "dumpasssembly";
    public const string DumpDomain = "dumpdomain";
    public const string Name2EE = "name2ee";

    public const string DumpMemory = "d";
    public const string DumpMemoryAsBytes = "db";
    public const string DumpMemoryAsChars = "dc";
    public const string DumpMemoryAsByteString = "da";
    public const string DumpMemoryAsCharString = "du";
    public const string DumpMemoryAsWords = "dw";
    public const string DumpMemoryAsDoubleWords = "dd";
    public const string DumpMemoryAsPointers = "dp";
    public const string DumpMemoryAsQuadWords = "dq";

    public const string Sos = "sos";
    public const string Ext = "ext";

    [GeneratedRegex(@$"((?:(?:{Sos}|{Ext})\s)?\S+)(.*)", RegexOptions.IgnoreCase)]
    public static partial Regex NormalizationRegex();

    public static string NormalizeCommand(string command)
    {
        if (NormalizationRegex().Match(command) is { Success: true } match)
        {
            var ranges = new Range[2];
            match.CopyGroupsRangesTo(ranges);
            return $"{command[ranges[0]].ToLower()}{command[ranges[1]]}";
        }

        return command.Trim();
    }
}