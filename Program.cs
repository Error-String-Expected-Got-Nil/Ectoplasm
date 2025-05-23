using System.Text.RegularExpressions;
using Ectoplasm.Lexing;
using Ectoplasm.Runtime;
using Ectoplasm.SimpleExpressions;

namespace Ectoplasm;

internal static class Program
{
    public static void Main()
    {
        var env = new LuaTable();
        
        while (true)
        {
            var line = Console.ReadLine();
            
            if (line is null or "") break;

            try
            {
                var tokens = Lexer.Lex(line)
                    .Where(token => token.Type is not (TokenType.Whitespace or TokenType.Comment))
                    .ToList();

                SimpleExpression exp;

                if (tokens is [{ Type: TokenType.Name }, { Type: TokenType.Assign }, _, ..])
                {
                    exp = SimpleExpressionParser.Parse(tokens[2..]);
                    var res = exp.Evaluate(env);
                    Console.WriteLine(res);
                    env[(string)tokens[0].Data!] = res;
                    continue;
                }

                exp = SimpleExpressionParser.Parse(tokens);
                Console.WriteLine(exp.Evaluate(env));
            }
            catch (LuaLexingException le)
            {
                Console.WriteLine($"Lexing error: {le.Message}");
            }
            catch (SimpleExpressionParsingException pe)
            {
                Console.WriteLine($"Parsing error: {pe.Message}");
            }
            catch (LuaRuntimeException re)
            {
                Console.WriteLine($"Runtime error: {re.Message}");
            }
        }
    }
}