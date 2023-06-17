using PoshDotnetDumpAnalyzeViewer;
using Xunit;

namespace PoshDotnetAnalyzerViewerTests;

public class DumpObjParsing
{
    [Fact]
    public void TestThatDumpObjIsParsedCorrectly()
    {
        var output = new[]
        {
            "> do 00007f9f1bfff138",
            "Name:        System.StackOverflowException",
            "MethodTable: 00007f9f540ff000",
            "EEClass:     00007f9f540d81a8",
            "Tracked Type: false",
            "Size:        128(0x80) bytes",
            "File:        /usr/share/dotnet/shared/Microsoft.NETCore.App/6.0.4/System.Private.CoreLib.dll",
            "Fields:",
            "              MT    Field   Offset                 Type VT     Attr            Value Name",
            "00007f9f54381c38  4000206        8 ...ection.MethodBase  0 instance 0000000000000000 _exceptionMethod", // dumpobj addr+fld // dumpvt if vt is 1
            "00007f9f540fd2e0  4000207       10        System.String  0 instance 0000000000000000 _message",         // dumpobj addr+fld
            "00007f9f54101250  4000208       18 ...tions.IDictionary  0 instance 0000000000000000 _data",            // dumpobj addr+fld
            "00007f9f540fec90  4000209       20     System.Exception  0 instance 0000000000000000 _innerException",  // dumpobj addr+fld
            "00007f9f540fd2e0  400020a       28        System.String  0 instance 0000000000000000 _helpURL",         // dumpobj addr+fld
            "00007f9f54b986b8  400020b       30        System.Byte[]  0 instance 0000000000000000 _stackTrace",       // dumpobj addr+fld
            "Thread:",
            ""
        };

        var lines = OutputParserExtensions.ParseAll<DumpObjectParser>(output, Commands.DumpObject);

        Assert.True(lines is
        [
            ObjectObjectAddressOutputLine { Address.Span: "00007f9f1bfff138" },
            TypeNameOutputLine { TypeName.Span: "System.StackOverflowException" },
            MethodTableOutputLine { MethodTable.Span: "00007f9f540ff000" },
            EEClassAddressOutputLine { EEClassAddress.Span: "00007f9f540d81a8" },
            {},
            {},
            {},
            {},
            {},
            DumpObjectOutputLine { MethodTable.Span: "00007f9f54381c38" },
            DumpObjectOutputLine { MethodTable.Span: "00007f9f540fd2e0" },
            DumpObjectOutputLine { MethodTable.Span: "00007f9f54101250" },
            DumpObjectOutputLine { MethodTable.Span: "00007f9f540fec90" },
            DumpObjectOutputLine { MethodTable.Span: "00007f9f540fd2e0" },
            DumpObjectOutputLine { MethodTable.Span: "00007f9f54b986b8" },
            {},
            {}
        ]);
    }
}