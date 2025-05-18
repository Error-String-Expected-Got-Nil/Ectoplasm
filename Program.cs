using System.Text.RegularExpressions;
using Ectoplasm.Lexing;

namespace Ectoplasm;

internal static class Program
{
    public static void Main(string[] args)
    {
        var testNumbers = """
                          3   345   0xff   0xBEBADA
                          3.0     3.1416     314.16e-2     0.31416E1     34e1     27e+2
                               0x0.1E  0xA23p-4   0X1.921FB54442D18P+1
                          """;
        
        foreach (Match match in Grammar.MatchNumber.Matches(testNumbers))
            Console.WriteLine(Grammar.IsFloatMatch(match)
                ? $"{match.Value} = {Grammar.ParseFloatMatch(match)}"
                : $"{match.Value} = {Grammar.ParseIntegerMatch(match)}");
    }
}