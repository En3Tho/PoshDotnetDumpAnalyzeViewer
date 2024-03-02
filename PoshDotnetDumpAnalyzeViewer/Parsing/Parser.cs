using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using PoshDotnetDumpAnalyzeViewer.Utilities;
using PoshDotnetDumpAnalyzeViewer.Views;

namespace PoshDotnetDumpAnalyzeViewer.Parsing;

static class RegexPatterns
{
    /// <summary>
    /// Digit
    /// </summary>
    [StringSyntax(StringSyntaxAttribute.Regex)]
    private const string D = @"-?\d+";

    /// <summary>
    /// HexChars only
    /// </summary>
    [StringSyntax(StringSyntaxAttribute.Regex)]
    private const string HChars = @"0-9a-fA-F";

    /// <summary>
    /// Hex
    /// </summary>
    [StringSyntax(StringSyntaxAttribute.Regex)]
    private const string H = @$"(?:0[xX])?[${HChars}]+";

    /// <summary>
    /// Address
    /// </summary>
    [StringSyntax(StringSyntaxAttribute.Regex)]
    private const string A = @"[0-9a-fA-F]{8,}"; // not sure about 8 but let's go with it for now

    /// <summary>
    /// Non white space
    /// </summary>
    [StringSyntax(StringSyntaxAttribute.Regex)]
    private const string S = @"\S+";

    /// <summary>
    /// Any amount of characters
    /// </summary>
    [StringSyntax(StringSyntaxAttribute.Regex)]
    private const string C = ".+";

    /// <summary>
    /// a to z and numbers
    /// </summary>
    [StringSyntax(StringSyntaxAttribute.Regex)]
    private const string azD = @"[a-z\d]+";

    /// <summary>
    /// Any amount of characters optional
    /// </summary>
    [StringSyntax(StringSyntaxAttribute.Regex)]
    private const string Co = ".*";

    /// <summary>
    /// White space
    /// </summary>
    [StringSyntax(StringSyntaxAttribute.Regex)]
    private const string WS = @"\s+";

    /// <summary>
    /// White space optional
    /// </summary>
    [StringSyntax(StringSyntaxAttribute.Regex)]
    private const string WSo = @"\s*";

    /// <summary>
    /// Type name (with white space quirk)
    /// </summary>
    [StringSyntax(StringSyntaxAttribute.Regex)]
    private const string T = @"(?:(?:,\s)|\S)+";

    /// <summary>
    /// Digit group
    /// </summary>
    private const string Dg = $"({D})";

    /// <summary>
    /// Hex group
    /// </summary>
    private const string Hg = $"({H})";

    /// <summary>
    /// Address group
    /// </summary>
    private const string Ag = $"({A})";

    /// <summary>
    /// Non-whitespace group
    /// </summary>
    private const string Sg = $"({S})";

    /// <summary>
    /// Non-whitespace optional group
    /// </summary>
    private const string Sgo = $"({S})*";

    /// <summary>
    /// Type name group
    /// </summary>
    private const string Tg = $"({T})";

    /// <summary>
    /// Type name optional group
    /// </summary>
    private const string Tgo = $"({T})*";

    // cmd (..,cmd)? <arg>? Description
    // S
    public const string Help =
        $@"{WSo}({azD}(?:,{WS}{azD})*)(?:<{C}>)?{C}";

    // ...   OsId ...
    // C WS  Hg    WS C
    public const string OsId =
        $"{C}{WS}{Hg}{WS}{C}";

    //                                                                              Lock
    // DBG           ID  OSID  ThreadOBJ  State  GC Mode  GC Alloc Context  Domain  Count  Apt   Exception
    // (D | "XXXX")g Dg  Hg    Ag         Hg     Sg       (A & ":" & A)g    Ag      Dg     Sg    Tg?
    public const string ClrThreads =
        $"{WSo}((?:XXXX)|{D}){WS}{Dg}{WS}{Hg}{WS}{Ag}{WS}{Hg}{WS}{Sg}{WS}({A}:{A}){WS}{Ag}{WS}{Dg}{WS}{Sg}{WSo}{Tgo}{WSo}";

    // Address  MT  Size ..rest
    // Ag       Ag  Dg
    public const string DumpHeap =
        $"{Ag}{WS}{Ag}{WS}{Dg}{C}";

    // MT  Count  TotalSize  Class Name
    // Ag  Dg     Dg         Tg
    public const string DumpHeapStatistics =
        $"{Ag}{WS}{Dg}{WS}{Dg}{WS}{Tg}{WSo}";

    // Address  MT  Size
    // Ag       Ag  Dg
    public const string ObjSize =
        $"{Ag}{WS}{Ag}{WS}{Dg}{WSo}";

    // MT  Count  TotalSize  Class Name
    // Ag  Dg     Dg         Tg
    public const string ObjSizeStatistics =
        $"{Ag}{WS}{Dg}{WS}{Dg}{WS}{Tg}{WSo}";

    // Index  SyncBlock  MonitorHeld  Recursion  Owning Thread Info  SyncBlock Owner
    // Dg     Ag         Dg           Dg         Ag Hg Dg            Ag Tg
    public const string SyncBlock =
        $"{WSo}{Dg}{WS}{Ag}{WS}{Dg}{WS}{Dg}{WS}{Ag}{WS}{Hg}{WS}{Dg}{WS}{Ag}{WS}{Tg}{WSo}";

    // Index  SyncBlock  MonitorHeld  Recursion  Owning Thread Info  SyncBlock Owner
    // Dg     Ag         Dg           Dg         Ag | none            Ag Tg
    public const string SyncBlockZero =
        $"{WSo}{Dg}{WS}{Ag}{WS}{Dg}{WS}{Dg}{WS}{Ag}{WS}({H}|none){WS}{Ag}{WS}{Tg}{WSo}";

    // Address MethodTable TypeName
    // Ag      Ag          Tg
    public const string DumpExceptions =
        $@"{WSo}{Ag}{WSo}{Ag}{WSo}{Tg}{WSo}";

    public static class DumpObject
    {
        public const string CmdWithAddress =
            $"do{WSo}{Ag}";

        public const string TypeName =
            $"Name:{WSo}{Tg}";

        public const string MethodTable =
            $"MethodTable:{WSo}{Ag}";

        public const string EEClass =
            $"EEClass:{WSo}{Ag}";

        // "MT  Field   Offset  Type VT     Attr            Value Name",
        // "Ag  Hg      Dg      Tg   Dg     Sg              Hg      Sg",
        public const string Main = $"{Ag}{WS}{Hg}{WS}{Hg}{WS}{Tg}{WS}{Dg}{WS}{Sg}{WS}{Hg}{WS}{Sg}";
    }

    public static class GCRoot
    {
        public const string Root =
            $@"{WSo}(?:{WS}->{WS})?{Ag}{WS}(\(strong handle\)|{S}){WSo}";

        // Thread c6c8:
        public const string ThreadId =
            $"Thread{WS}{Hg}:{WSo}";
    }

    public static class DumpMethodTable
    {
        public const string EEClass =
            $"EEClass:{WSo}{Ag}";

        public const string TypeName =
            $"Name:{WSo}{Tg}";

        public const string Module =
            $"Module:{WSo}{Tg}";
    }

    public static class DumpClass
    {
        public const string TypeName =
            $"Class Name:{WSo}{Tg}";

        public const string ParentClass =
            $"Parent Class:{WSo}{Tg}";

        public const string Module =
            $"Module:{WSo}{Tg}";

        public const string MethodTable =
            $"Method Table:{WSo}{Ag}";
    }

    public static class DumpModule
    {
        public const string Assembly =
            $"Assembly:{WSo}{Ag}";
    }

    public static class DumpAssembly
    {
        public const string ParentDomain =
            $"Parent Domain:{WSo}{Ag}";

        // 00007f003dfe4020    /usr/share/dotnet/shared/Microsoft.NETCore.App/5.0.17/System.Private.CoreLib.dll
        public const string Module =
            $"^{WSo}{Ag}{WSo}{T}{WSo}";
    }

    public static class DumpDomain
    {
        public const string Assembly =
            $"Assembly:{WSo}{Ag}";

        public const string Module =
            $"^{WSo}{Ag}{WSo}{Tgo}";
    }

    public static class Name2EE
    {
        public const string Module =
            $"Module:{WSo}{Ag}";

        public const string MethodTable =
            $"MethodTable:{WSo}{Ag}";

        public const string EEClass =
            $"EEClass:{WSo}{Ag}";

        public const string TypeName =
            $"Name:{WSo}{Tg}";
    }

    public static class PrintException
    {
        public const string ExceptionObject =
            $"Exception object:{WSo}{Ag}";

        public const string ExceptionType =
            $"Exception type:{WSo}{Tg}";

        public const string InnerException =
            $"InnerException:{C}Use printexception{WSo}{Ag}{C}";
    }

    public static class ParallelStacks
    {
        public const string ThreadCount =
            $"^{WS}{Dg}{WS}{C}";

        public const string ThreadNames =
            $"{WS}~~~~{WSo}([{HChars},]+)";
    }

    public static class ClrStack
    {
        public const string ParameterOrLocalAddress =
            $"{WS}{C}{WS}={WS}{Hg}";
    }
}

public interface IOutputParser
{
    static abstract OutputLine Parse(string line, string command);
}

public partial class HelpParser : IOutputParser
{
    [GeneratedRegex(RegexPatterns.Help)]
    public static partial Regex Regex();

    public static OutputLine Parse(string line, string _)
    {
        if (Regex().Match(line) is { Success: true } match)
        {
            var commands = match.Groups[1].Value.Split(",", StringSplitOptions.TrimEntries);
            return new HelpOutputLine(line, commands);
        }
        
        return new(line);
    }

    public static string[] GetCommandsFromLine(string line)
    {
        return Regex().Match(line).Groups[1].Value.Split(",", StringSplitOptions.TrimEntries);
    }
}

public partial class DumpHeapParser : IOutputParser
{
    [GeneratedRegex(RegexPatterns.DumpHeap)]
    public static partial Regex DumpHeapRegex();

    [GeneratedRegex(RegexPatterns.DumpHeapStatistics)]
    public static partial Regex DumpHeapStatisticsRegex();

    public static OutputLine Parse(string line, string _)
    {
        if (GetDumpHeapStatisticsHeaderRanges(line) is {} dumpHeapStatisticsRanges)
            return new DumpHeapStatisticsOutputLine(line, line[dumpHeapStatisticsRanges.MethodTable], line[dumpHeapStatisticsRanges.ClassName]);

        if (GetDumpHeapHeaderRanges(line) is {} dumpHeapRanges)
            return new DumpHeapOutputLine(line, line[dumpHeapRanges.Address], line[dumpHeapRanges.MethodTable]);

        return new(line);
    }

    public static DumpHeapRanges? GetDumpHeapHeaderRanges(string line)
    {
        if (DumpHeapRegex().Match(line) is { Success: true } match)
        {
            var ranges = new Range[3];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0], ranges[1], ranges[2]);
        }

        return default;
    }

    public static DumpHeapStatisticsRanges? GetDumpHeapStatisticsHeaderRanges(string line)
    {
        if (DumpHeapStatisticsRegex().Match(line) is { Success: true } match)
        {
            var ranges = new Range[4];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0], ranges[1], ranges[2], ranges[3]);
        }

        return default;
    }
}

public partial class ObjSizeParser : IOutputParser
{
    [GeneratedRegex(RegexPatterns.ObjSize)]
    public static partial Regex ObjSizeRegex();

    [GeneratedRegex(RegexPatterns.ObjSizeStatistics)]
    public static partial Regex ObjSizeStatisticsRegex();

    public static OutputLine Parse(string line, string _)
    {
        if (GetObjSizeStatisticsHeaderRanges(line) is { } objSizeStatisticsRanges)
            return new ObjSizeStatisticsOutputLine(line, line[objSizeStatisticsRanges.MethodTable], line[objSizeStatisticsRanges.ClassName]);

        if (GetObjSizeHeaderRanges(line) is {} objSizeRanges)
            return new ObjSizeOutputLine(line, line[objSizeRanges.Address], line[objSizeRanges.MethodTable]);

        return new(line);
    }

    public static ObjSizeRanges? GetObjSizeHeaderRanges(string line)
    {
        if (ObjSizeRegex().Match(line) is { Success: true } match)
        {
            var ranges = new Range[3];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0], ranges[1], ranges[2]);
        }

        return default;
    }

    public static ObjSizeStatisticsRanges? GetObjSizeStatisticsHeaderRanges(string line)
    {
        if (ObjSizeStatisticsRegex().Match(line) is { Success: true } match)
        {
            var ranges = new Range[4];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0], ranges[1], ranges[2], ranges[3]);
        }

        return default;
    }
}

public partial class SetThreadParser : IOutputParser
{
    [GeneratedRegex(RegexPatterns.OsId)]
    public static partial Regex Regex();

    public static OutputLine Parse(string line, string _)
    {
        if (GetRanges(line) is {} ranges)
            return new OsThreadIdOutputLine(line, line[ranges.OsThreadId]);

        return new(line);
    }

    public static OsThreadIdRanges? GetRanges(string line)
    {
        if (Regex().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }
}

public partial class ClrThreadsParser : IOutputParser
{
    [GeneratedRegex(RegexPatterns.ClrThreads)]
    public static partial Regex Regex();

    public static OutputLine Parse(string line, string _)
    {
        if (GetRanges(line) is {} ranges)
            return new ClrThreadsOutputLine(line, line[ranges.OsId], line[ranges.Id], line[ranges.State]);

        return new(line);
    }

    public static ClrThreadsRanges? GetRanges(string line)
    {
        if (Regex().Match(line) is { Success: true } match)
        {
            var ranges = new Range[11];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0], ranges[1], ranges[2], ranges[3], ranges[4], ranges[5], ranges[6], ranges[7], ranges[8], ranges[9], ranges[10]);
        }

        return default;
    }
}

public partial class SyncBlockParser : IOutputParser
{
    [GeneratedRegex(RegexPatterns.SyncBlock)]
    public static partial Regex Regex();

    [GeneratedRegex(RegexPatterns.SyncBlockZero)]
    public static partial Regex RegexZero();

    public static OutputLine Parse(string line, string _)
    {
        if (GetRanges(line) is {} ranges)
            return new SyncBlockOutputLine(line, line[ranges.OwningThreadOsId], line[ranges.SyncBlockOwnerAddress], line[ranges.SyncBlockOwnerType], line[ranges.SyncBlock], line[ranges.Index]);

        if (GetZeroRanges(line) is {} zeroRanges)
        {
            // special case lines like
            // 64 0000000000000000            0         0 0000000000000000     none           0 Free
            if (line.AsSpan()[zeroRanges.SyncBlock].IndexOfAnyExcept('0') > -1)
            {
                return new SyncBlockZeroOutputLine(line, line[zeroRanges.SyncBlockOwnerAddress], line[zeroRanges.SyncBlockOwnerType], line[zeroRanges.SyncBlock], line[zeroRanges.Index]);
            }
        }

        return new(line);
    }

    public static SyncBlockRanges? GetRanges(string line)
    {
        if (Regex().Match(line) is { Success: true } match)
        {
            var ranges = new Range[9];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0], ranges[1], ranges[2], ranges[3], ranges[4], ranges[5], ranges[6], ranges[7], ranges[8]);
        }

        return default;
    }

    public static SyncBlockZeroRanges? GetZeroRanges(string line)
    {
        if (RegexZero().Match(line) is { Success: true } match)
        {
            var ranges = new Range[8];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0], ranges[1], ranges[2], ranges[3], ranges[4], ranges[5], ranges[6], ranges[7]);
        }

        return default;
    }
}

public partial class GCRootParser : IOutputParser
{
    [GeneratedRegex(RegexPatterns.GCRoot.Root)]
    public static partial Regex GCRootRegex();

    [GeneratedRegex(RegexPatterns.GCRoot.ThreadId)]
    public static partial Regex ThreadIdRegex();

    public static OutputLine Parse(string line, string _)
    {
        if (GetGCRootRanges(line) is { } ranges)
        {
            if (line.AsSpan()[ranges.TypeName].Contains("strong handle", StringComparison.Ordinal))
                return new ObjectAddressOutputLine(line, line[ranges.Address]);

            return new GCRootOutputLine(line, line[ranges.Address], line[ranges.TypeName]);
        }

        if (GetOsThreadIdRanges(line) is { } osThreadIdRanges)
            return new OsThreadIdOutputLine(line, line[osThreadIdRanges.OsThreadId]);

        return new(line);
    }

    public static OsThreadIdRanges? GetOsThreadIdRanges(string line)
    {
        if (ThreadIdRegex().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }

    public static GCRootRanges? GetGCRootRanges(string line)
    {
        if (GCRootRegex().Match(line) is { Success: true } match)
        {
            var ranges = new Range[2];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0], ranges[1]);
        }

        return default;
    }
}

public partial class DumpExceptionsParser : IOutputParser
{
    [GeneratedRegex(RegexPatterns.DumpExceptions)]
    public static partial Regex Regex();

    public static OutputLine Parse(string line, string _)
    {
        if (GetRanges(line) is { } ranges)
        {
            return new DumpExceptionsOutputLine(line, line[ranges.Address], line[ranges.MethodTable], line[ranges.TypeName]);
        }

        return new(line);
    }

    public static DumpExceptionRanges? GetRanges(string line)
    {
        if (Regex().Match(line) is { Success: true } match)
        {
            var ranges = new Range[3];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0], ranges[1], ranges[2]);
        }

        return default;
    }
}

public partial class DumpObjectParser : IOutputParser
{
    [GeneratedRegex(RegexPatterns.DumpObject.CmdWithAddress)]
    public static partial Regex CmdWithAddress();

    [GeneratedRegex(RegexPatterns.DumpObject.MethodTable)]
    public static partial Regex MethodTable();

    [GeneratedRegex(RegexPatterns.DumpObject.TypeName)]
    public static partial Regex TypeName();

    [GeneratedRegex(RegexPatterns.DumpObject.EEClass)]
    public static partial Regex EEClass();

    [GeneratedRegex(RegexPatterns.DumpObject.Main)]
    public static partial Regex Main();

    public static OutputLine Parse(string line, string _)
    {
        if (GetMainRanges(line) is {} mainRanges)
            return new DumpObjectOutputLine(line, line[mainRanges.Value], line[mainRanges.MethodTable]);

        if (GetObjectAddressRanges(line) is {} objectAddressRanges)
            return new ObjectAddressOutputLine(line, line[objectAddressRanges.Address]);

        if (GetMethodTableRanges(line) is {} methodTableRanges)
            return new MethodTableOutputLine(line, line[methodTableRanges.MethodTable]);

        if (GetTypeNameRanges(line) is {} typeNameRanges)
            return new TypeNameOutputLine(line, line[typeNameRanges.TypeName]);

        if (GetEEClassRanges(line) is {} eeClassRanges)
            return new EEClassAddressOutputLine(line, line[eeClassRanges.EEClass]);

        return new(line);
    }

    public static ObjectAddressRanges? GetObjectAddressRanges(string line)
    {
        if (CmdWithAddress().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }

    public static MethodTableRanges? GetMethodTableRanges(string line)
    {
        if (MethodTable().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }

    public static TypeNameRanges? GetTypeNameRanges(string line)
    {
        if (TypeName().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }

    public static EEClassAddressRanges? GetEEClassRanges(string line)
    {
        if (EEClass().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }

    public static DumpObjectRanges? GetMainRanges(string line)
    {
        if (Main().Match(line) is { Success: true } match)
        {
            var ranges = new Range[8];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0], ranges[1], ranges[2], ranges[3], ranges[4], ranges[5], ranges[6], ranges[7]);
        }

        return default;
    }
}

public partial class DumpMethodTableParser : IOutputParser
{
    [GeneratedRegex(RegexPatterns.DumpMethodTable.Module)]
    public static partial Regex ModuleAddress();

    [GeneratedRegex(RegexPatterns.DumpMethodTable.EEClass)]
    public static partial Regex EEClass();
    
    [GeneratedRegex(RegexPatterns.DumpMethodTable.TypeName)]
    public static partial Regex TypeName();

    public static OutputLine Parse(string line, string _)
    {
        if (GetModuleAddressRanges(line) is {} moduleAddressRanges)
            return new ModuleAddressOutputLine(line, line[moduleAddressRanges.Module]);

        if (GetTypeNameRanges(line) is {} typeNameRanges)
            return new TypeNameOutputLine(line, line[typeNameRanges.TypeName]);

        if (GetEEClassRanges(line) is {} eeClassRanges)
            return new EEClassAddressOutputLine(line, line[eeClassRanges.EEClass]);

        return new(line);
    }

    public static ModuleAddressRanges? GetModuleAddressRanges(string line)
    {
        if (ModuleAddress().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }

    public static TypeNameRanges? GetTypeNameRanges(string line)
    {
        if (TypeName().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }

    public static EEClassAddressRanges? GetEEClassRanges(string line)
    {
        if (EEClass().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }
}

public partial class DumpClassParser : IOutputParser
{
    [GeneratedRegex(RegexPatterns.DumpClass.Module)]
    public static partial Regex Module();

    [GeneratedRegex(RegexPatterns.DumpClass.ParentClass)]
    public static partial Regex ParentClass();

    [GeneratedRegex(RegexPatterns.DumpClass.TypeName)]
    public static partial Regex TypeName();

    [GeneratedRegex(RegexPatterns.DumpClass.MethodTable)]
    public static partial Regex MethodTable();

    public static OutputLine Parse(string line, string _)
    {
        if (GetModuleAddressRanges(line) is {} moduleAddressRanges)
            return new ModuleAddressOutputLine(line, line[moduleAddressRanges.Module]);

        if (GetTypeNameRanges(line) is {} typeNameRanges)
            return new TypeNameOutputLine(line, line[typeNameRanges.TypeName]);

        if (GetParentClassRanges(line) is {} eeClassRanges)
            return new EEClassAddressOutputLine(line, line[eeClassRanges.EEClass]);
        
        if (GetMethodTableRanges(line) is {} methodTableRanges)
            return new MethodTableOutputLine(line, line[methodTableRanges.MethodTable]);
        
        return new(line);
    }

    public static ModuleAddressRanges? GetModuleAddressRanges(string line)
    {
        if (Module().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }

    public static TypeNameRanges? GetTypeNameRanges(string line)
    {
        if (TypeName().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }

    public static EEClassAddressRanges? GetParentClassRanges(string line)
    {
        if (ParentClass().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }
    
    public static MethodTableRanges? GetMethodTableRanges(string line)
    {
        if (MethodTable().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }
}

public partial class DumpModuleParser : IOutputParser
{
    [GeneratedRegex(RegexPatterns.DumpModule.Assembly)]
    public static partial Regex AssemblyAddress();


    public static OutputLine Parse(string line, string _)
    {
        if (GetAssemblyAddressRanges(line) is {} assemblyAddressRanges)
            return new AssemblyAddressOutputLine(line, line[assemblyAddressRanges.Assembly]);

        return new(line);
    }

    public static AssemblyAddressRanges? GetAssemblyAddressRanges(string line)
    {
        if (AssemblyAddress().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }
}

public partial class DumpAssemblyParser : IOutputParser
{
    [GeneratedRegex(RegexPatterns.DumpAssembly.Module)]
    public static partial Regex ModuleAddress();

    [GeneratedRegex(RegexPatterns.DumpAssembly.ParentDomain)]
    public static partial Regex ParentDomain();

    public static OutputLine Parse(string line, string _)
    {
        if (GetModuleAddressRanges(line) is {} moduleAddressRanges)
            return new ModuleAddressOutputLine(line, line[moduleAddressRanges.Module]);

        if (GetDomainAddressRanges(line) is {} domainAddressRanges)
            return new DomainAddressOutputLine(line, line[domainAddressRanges.Domain]);

        return new(line);
    }

    public static ModuleAddressRanges? GetModuleAddressRanges(string line)
    {
        if (ModuleAddress().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }

    public static DomainAddressRanges? GetDomainAddressRanges(string line)
    {
        if (ParentDomain().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }
}

public partial class DumpDomainParser : IOutputParser
{
    [GeneratedRegex(RegexPatterns.DumpDomain.Module)]
    public static partial Regex Module();

    [GeneratedRegex(RegexPatterns.DumpDomain.Assembly)]
    public static partial Regex Assembly();

    public static OutputLine Parse(string line, string _)
    {
        if (GetModuleAddressRanges(line) is {} moduleAddressRanges)
            return new ModuleAddressOutputLine(line, line[moduleAddressRanges.Module]);

        if (GetAssemblyAddressRanges(line) is {} assemblyAddressRanges)
            return new AssemblyAddressOutputLine(line, line[assemblyAddressRanges.Assembly]);

        return new(line);
    }

    public static ModuleAddressRanges? GetModuleAddressRanges(string line)
    {
        if (Module().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }

    public static AssemblyAddressRanges? GetAssemblyAddressRanges(string line)
    {
        if (Assembly().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }
}

public partial class Name2EEParser : IOutputParser
{
    [GeneratedRegex(RegexPatterns.Name2EE.Module)]
    public static partial Regex Module();

    [GeneratedRegex(RegexPatterns.Name2EE.EEClass)]
    public static partial Regex EEClass();

    [GeneratedRegex(RegexPatterns.Name2EE.TypeName)]
    public static partial Regex TypeName();

    [GeneratedRegex(RegexPatterns.Name2EE.MethodTable)]
    public static partial Regex MethodTable();

    public static OutputLine Parse(string line, string _)
    {
        if (GetModuleAddressRanges(line) is {} moduleAddressRanges)
            return new ModuleAddressOutputLine(line, line[moduleAddressRanges.Module]);

        if (GetTypeNameRanges(line) is {} typeNameRanges)
            return new TypeNameOutputLine(line, line[typeNameRanges.TypeName]);

        if (GetParentClassRanges(line) is {} eeClassRanges)
            return new EEClassAddressOutputLine(line, line[eeClassRanges.EEClass]);

        if (GetMethodTableRanges(line) is {} methodTableRanges)
            return new MethodTableOutputLine(line, line[methodTableRanges.MethodTable]);

        return new(line);
    }

    public static ModuleAddressRanges? GetModuleAddressRanges(string line)
    {
        if (Module().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }

    public static TypeNameRanges? GetTypeNameRanges(string line)
    {
        if (TypeName().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }

    public static EEClassAddressRanges? GetParentClassRanges(string line)
    {
        if (EEClass().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }

    public static MethodTableRanges? GetMethodTableRanges(string line)
    {
        if (MethodTable().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }
}

public partial class PrintExceptionParser : IOutputParser
{
    [GeneratedRegex(RegexPatterns.PrintException.ExceptionObject)]
    public static partial Regex ExceptionObject();

    [GeneratedRegex(RegexPatterns.PrintException.ExceptionType)]
    public static partial Regex ExceptionType();

    [GeneratedRegex(RegexPatterns.PrintException.InnerException)]
    public static partial Regex InnerException();

    public static OutputLine Parse(string line, string _)
    {
        if (GetExceptionObjectRanges(line) is {} exceptionObjectRanges)
            return new ExceptionObjectAddressOutputLine(line, line[exceptionObjectRanges.Address]);

        if (GetExceptionTypeRanges(line) is {} typeNameRanges)
            return new TypeNameOutputLine(line, line[typeNameRanges.TypeName]);

        if (GetInnerExceptionRanges(line) is {} innerExceptionRanges)
            return new ExceptionObjectAddressOutputLine(line, line[innerExceptionRanges.Address]);

        return new(line);
    }

    public static ObjectAddressRanges? GetExceptionObjectRanges(string line)
    {
        if (ExceptionObject().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }

    public static TypeNameRanges? GetExceptionTypeRanges(string line)
    {
        if (ExceptionType().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }

    public static ObjectAddressRanges? GetInnerExceptionRanges(string line)
    {
        if (InnerException().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }
}

public partial class ParallelStacksParser : IOutputParser
{
    [GeneratedRegex(RegexPatterns.ParallelStacks.ThreadCount)]
    public static partial Regex ThreadCount();

    [GeneratedRegex(RegexPatterns.ParallelStacks.ThreadNames)]
    public static partial Regex ThreadNames();

    public static OutputLine Parse(string line, string command)
    {
        var ranges = ThreadNamesRanges(line) ?? default;
        return new ParallelStacksOutputLine(line, line[ranges.OsThreadIds].Split(',', StringSplitOptions.RemoveEmptyEntries));
    }

    public static bool TryParseThreadCount(string line, out int threadCount)
    {
        if (ThreadCount().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            threadCount = int.Parse(line.AsMemory(ranges[0]).Span);
            return true;
        }

        threadCount = 0;
        return false;
    }

    public static ParallelStacksRanges? ThreadNamesRanges(string line)
    {
        if (ThreadNames().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }

    public static bool IsThreadNames(string line) => ThreadNames().IsMatch(line);

    public static string[] ShrinkParallelStacksOutput(string[] lines)
    {
        var reducedStack = new List<string>(lines.Length / 10);
        var currentThreadCount = -1;
        var firstLine = "";
        var lastLine = "";

        foreach (var line in lines)
        {
            if (IsThreadNames(line))
            {
                reducedStack.Add(firstLine);
                reducedStack.Add(lastLine);
                reducedStack.Add(line);
                currentThreadCount = -1;
                continue;
            }

            if (TryParseThreadCount(line, out var threadCount))
            {
                if (currentThreadCount != -1 && currentThreadCount != threadCount)
                {
                    reducedStack.Add(firstLine);
                    if (!ReferenceEquals(firstLine, lastLine))
                    {
                        reducedStack.Add(lastLine);
                    }
                }

                if (currentThreadCount != threadCount)
                {
                    firstLine = line;
                    currentThreadCount = threadCount;
                }

                lastLine = line;
                continue;
            }

            reducedStack.Add(line);
        }

        return reducedStack.ToArray();
    }
}

public partial class ClrStackParser : IOutputParser
{
    [GeneratedRegex(RegexPatterns.ClrStack.ParameterOrLocalAddress)]
    public static partial Regex ParameterOrLocalAddress();

    public static OutputLine Parse(string line, string _)
    {
        if (ParameterOrLocalAddressRanges(line) is {} objectAddressRanges)
            return new ObjectAddressOutputLine(line, line[objectAddressRanges.Address]);

        return new(line);
    }

    public static ObjectAddressRanges? ParameterOrLocalAddressRanges(string line)
    {
        if (ParameterOrLocalAddress().Match(line) is { Success: true } match)
        {
            var ranges = new Range[1];
            match.CopyGroupsRangesTo(ranges);
            return new(ranges[0]);
        }

        return default;
    }
}