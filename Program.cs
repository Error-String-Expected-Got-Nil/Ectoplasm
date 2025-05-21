using System.Text.RegularExpressions;
using Ectoplasm.Lexing;
using Ectoplasm.Runtime;

namespace Ectoplasm;

internal static class Program
{
    public static void Main(string[] args)
    {
        var table = new LuaTable
        {
            ["test"u8] = "value"u8
        };
        
        Console.WriteLine(table["test"u8].StringUtf16);
    }
}