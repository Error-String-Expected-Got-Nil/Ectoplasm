using System.Text.RegularExpressions;
using Ectoplasm.Lexing;
using Ectoplasm.Runtime;
using Ectoplasm.SimpleExpressions;

namespace Ectoplasm;

internal static class Program
{
    public static void Main()
    {
        var expStr = "1 + 1";

        var exp = SimpleExpressionParser.Parse(Lexer.Lex(expStr)
            .Where(token => token.Type is not (TokenType.Whitespace or TokenType.Comment)));
        
        
    }
}