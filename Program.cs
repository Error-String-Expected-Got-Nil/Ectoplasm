using System.Diagnostics;
using Ectoplasm.Lexing;
using Ectoplasm.Parsing;
using Ectoplasm.Parsing.Statements;

namespace Ectoplasm;

internal static class Program
{
    public static void Main()
    {
        var sourceStr =
            """
            local function fizzbuzz()
                for i = 1, 100 do
                    if i % 15 == 0 then
                        print("FizzBuzz")
                    elseif i % 3 == 0 then
                        print("Fizz")
                    elseif i % 5 == 0 then
                        print("Buzz")
                    else
                        print(i)
                    end
                end
            end
            fizzbuzz()
            print "test"
            """;

        var start = Stopwatch.GetTimestamp();
        
        var tokens = Lexer.Lex(sourceStr, "fizzbuzz.lua").ToList();
        using var source = tokens
            .Where(token => token.Type is not (TokenType.Whitespace or TokenType.Comment))
            .GetEnumerator();
        source.MoveNext();
        var block = Parser.ParseBlock(source, "fizzbuzz.lua");
        var proto = ScopeAnalyzer.AnalyzeChunk(block, "fizzbuzz.lua");
        
        var end = Stopwatch.GetTimestamp();

        var debug = Statement.GetBlockDebugString(block);
        
        Console.WriteLine(debug);
        Console.WriteLine($"Parse time: {(end - start) / (Stopwatch.Frequency / 1000.0):F2} ms");
    }
}