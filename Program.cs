using System.Text.RegularExpressions;
using Ectoplasm.Lexing;
using Ectoplasm.Runtime;
using Ectoplasm.SimpleExpressions;

namespace Ectoplasm;

internal static class Program
{
    public static void Main()
    {
        var expStr = "2 * (test + 2)";

        var exp = SimpleExpressionParser.Parse(Lexer.Lex(expStr)
            .Where(token => token.Type is not (TokenType.Whitespace or TokenType.Comment)));

        var env = new LuaTable
        {
            ["test"] = 4
        };

        Console.WriteLine(exp.Evaluate(env));
    }
}