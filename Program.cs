using System.Text.RegularExpressions;
using Ectoplasm.Lexing;
using Ectoplasm.Runtime;

namespace Ectoplasm;

internal static class Program
{
    public static void Main()
    {
        var lua = """
                  local str = [[
                  multi-line
                  long string]]
                  int=13
                  hexFloat= 0x1.fp10
                  -- comment
                  function test()
                    print(--[==[block ]]comment]==]"test!")
                    print([[long string]])
                  end
                  """;
        
        var types = new List<string>();
        var locs = new List<string>();
        var data = new List<string>();

        foreach (var token in Lexer.Lex(lua).Where(t => t.Type is not TokenType.Whitespace))
        {
            var formatted = token.FormattedData();
            types.Add(formatted.Type);
            locs.Add(formatted.Location);
            data.Add(formatted.Data);
        }

        var typePadding = types.Max(str => str.Length) + 3;
        var locPadding = locs.Max(str => str.Length) + 3;

        for (var i = 0; i < types.Count; i++) 
            Console.WriteLine($"{types[i].PadRight(typePadding)}{locs[i].PadRight(locPadding)}{data[i]}");
    }
}