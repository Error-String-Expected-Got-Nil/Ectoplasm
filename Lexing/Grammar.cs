using System.Text.RegularExpressions;
using Ectoplasm.Lexing.Tokens;
using KTrie;

using static Ectoplasm.Lexing.Tokens.TokenType;

namespace Ectoplasm.Lexing;

/// <summary>
/// Static information for matching tokens from raw code during lexing.
/// </summary>
public static partial class Grammar
{
    /// <summary>
    /// Trie relating string for keywords to the token type of that keyword.
    /// </summary>
    public static readonly TrieDictionary<TokenType> Keywords = new()
    {
        // Keywords
        { "do", Do },
        { "end", End },
        { "if", If },
        { "then", Then },
        { "elseif", Elseif },
        { "else", Else },
        { "for", For },
        { "in", In },
        { "while", While },
        { "repeat", Repeat },
        { "until", Until },
        { "goto", Goto },
        { "function", Function },
        { "local", Local },
        { "break", Break },
        { "return", Return },
        
        // Logical keywords
        { "nil", Nil },
        { "false", False },
        { "true", True },
        { "and", And },
        { "or", Or },
        { "not", Not }
    };

    /// <summary>
    /// Trie relating sequences of symbols to the token type of that sequence.
    /// </summary>
    public static readonly TrieDictionary<TokenType> Symbols = new()
    {
        // Delimiter symbols
        { ";", Statement },
        { ",", Separator },
        { "...", Varargs },
        { "::", LabelSep },
        { "(", OpenExp },
        { ")", CloseExp },
        { "[", OpenIndex },
        { "]", CloseIndex },
        { "{", OpenTable },
        { "}", CloseTable },
        { "--", Comment },
        
        // Operator symbols
        { "+", Add },
        { "-", Sub },
        { "*", Mul },
        { "/", Div },
        { "//", IntDiv },
        { "^", Exp },
        { "%", Mod },
        { "&", BitwiseAnd },
        { "~", BitwiseXor },
        { "|", BitwiseOr },
        { ">>", ShiftRight },
        { "<<", ShiftLeft },
        { "..", Concat },
        { "<", LessThan },
        { "<=", LessThanOrEq },
        { ">", GreaterThan  },
        { ">=", GreaterThanOrEq },
        { "==", EqualTo },
        { "~=", NotEqualTo },
        { ".", IndexName },
        { ":", IndexMethod }
    };

    /// <summary>
    /// Maximum number of entries in the trie which start with the same character.
    /// </summary>
    // Somewhat naive way of computing this, but this doesn't need to be especially efficient; it's a static readonly
    // that only runs once.
    public static readonly int SymbolsMaxPrefix = Symbols.Keys.Max(key => Symbols.StartsWith([key[0]]).Count());
    
    /// <summary>
    /// Matches a Lua 'name', which can be used for labels, identifiers, keywords, table keys, etc.
    /// </summary>
    [GeneratedRegex(@"[_a-zA-Z]\w*")]
    public static partial Regex MatchName { get; }
    
    /// <summary>
    /// Matches both integer and floating point decimal and hex numbers, all valid in Lua. Captures:
    /// 0: Hex identifier 0x or 0X if present. If empty, number is not hexadecimal.
    /// 1 through 3: Empty if not hex.
    /// 1: Hex significand.
    /// 2: Hex fraction, if present, with leading period.
    /// 3: Decimal exponent (power of 2), if present, with leading p or P and possibly a sign following it.
    /// 4 through 6: Empty if hex.
    /// 4: Decimal significand.
    /// 5: Decimal fraction, if present, with leading period.
    /// 6: Decimal exponent (power of 10), if present, with leading e or E and possibly a sign following it.
    /// </summary>
    [GeneratedRegex(@"(0[xX])([\da-fA-F]+)(\.[\da-fA-F]+)?([pP][+-]?\d+)?|(\d+)(\.\d+)?([eE][+-]?\d+)?")]
    public static partial Regex MatchNumber { get; }
    
    /// <summary>
    /// Matches any sequence of symbols used in Lua.
    /// </summary>
    [GeneratedRegex("[%-\\/:->{-~\\\"#\\[\\]]")]
    public static partial Regex MatchSymbols { get; }
    
    /// <summary>
    /// Matches any sequences of 'text' characters, that could make up a Name or number literal.
    /// </summary>
    [GeneratedRegex(@"[\w][\w\.+-]*")]
    public static partial Regex MatchText { get; }

    /// <summary>
    /// Checks if a character is a valid character for starting a text sequence (a potential Name or number literal).
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns>True if it is a digit, letter, or underscore, false if not.</returns>
    public static bool IsTextStart(char c) 
        => c is >= 'a' and <= 'z' 
            or >= 'A' and <= 'Z' 
            or >= '0' and <= '9'
            or '_';
    
    /// <summary>
    /// Checks if a character is a symbol used in Lua.
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns>True if it is a valid symbol used in Lua, false if not.</returns>
    // Checks are ordered with the widest character ranges first, single characters last.
    // These are ASCII character ranges.
    public static bool IsSymbol(char c)
        => c is >= '%' and <= '/'
            or >= ':' and <= '>'
            or >= '{' and <= '~'
            or '"'
            or '#'
            or '['
            or ']';
    
    // TODO: Parse numbers matched by that horrific MatchNumber regex.

    /// <summary>
    /// For use with the MatchNumber regex. Checks if match is a hexadecimal number.
    /// </summary>
    /// <param name="match">Match to check.</param>
    /// <returns>True if match is a hexadecimal number, false if not.</returns>
    public static bool IsHexMatch(Match match) => match.Captures[0].Length != 0;

    /// <summary>
    /// For use with the MatchNumber regex. Checks if match represents a float or an integer.
    /// </summary>
    /// <param name="match"></param>
    /// <returns></returns>
    public static bool IsFloatMatch(Match match)
        => IsHexMatch(match)
            ? match.Captures[2].Length != 0 || match.Captures[3].Length != 0
            : match.Captures[5].Length != 0 || match.Captures[6].Length != 0;

    /// <summary>
    /// For use with the MatchNumber regex. Gets the integer value of the match. 
    /// </summary>
    /// <param name="match">Match to parse.</param>
    /// <returns>Integer value of match. If match has floating-point components, they will be ignored.</returns>
    public static long ParseIntegerMatch(Match match) 
        => IsHexMatch(match) 
            ? ParseHexInteger(match) 
            : ParseDecimalInteger(match);

    /// <summary>
    /// For use with the MatchNumber regex. Gets the double-precision floating point value of the match.
    /// </summary>
    /// <param name="match">Match to parse.</param>
    /// <returns>Double-precision floating point of match. If match has no floating point components, it will be the
    /// integer value of the match in floating point form.</returns>
    public static double ParseFloatMatch(Match match)
        => IsHexMatch(match)
            ? ParseHexFloat(match)
            : ParseDecimalFloat(match);
    
    /// <summary>
    /// For use with the MatchNumber regex. Parses a decimal integer number from a match.
    /// </summary>
    /// <param name="match">Match to parse.</param>
    /// <returns>64-bit integer value of matched number.</returns>
    private static long ParseDecimalInteger(Match match)
        => Convert.ToInt64(match.Captures[4].Value, 10);

    /// <summary>
    /// For use with the MatchNumber regex. Parses a decimal floating point number from a match.
    /// </summary>
    /// <param name="match">Match to parse.</param>
    /// <returns>Double-precision floating point value of matched number.</returns>
    private static double ParseDecimalFloat(Match match)
        => double.Parse(match.Captures[4].Value + match.Captures[5].Value + match.Captures[6].Value);

    /// <summary>
    /// For use with the MatchNumber regex. Parses a hexadecimal integer number from a match.
    /// </summary>
    /// <param name="match">Match to parse.</param>
    /// <returns>64-bit integer value of matched number.</returns>
    private static long ParseHexInteger(Match match)
        => Convert.ToInt64(match.Captures[1].Value, 16);

    /// <summary>
    /// Lookup dictionary for the value of a hex character as a double.
    /// </summary>
    private static readonly Dictionary<char, double> HexLookupDict = new()
    {
        { '0', 0.0 }, { '1', 1.0  }, { '2', 2.0  }, { '3', 3.0 }, { '4', 4.0 }, { '5', 5.0 }, { '6', 6.0 }, 
        { '7', 7.0 }, { '8', 8.0 }, { '9', 9.0 }, { 'a', 10.0 }, { 'b', 11.0 }, { 'c', 12.0 }, { 'd', 13.0 }, 
        { 'e', 14.0 }, { 'f', 15.0 }, { 'A', 10.0 }, { 'B', 11.0 }, { 'C', 12.0 }, { 'D', 13.0 }, { 'E', 14.0 },
        { 'F', 15.0 }
    };

    // TODO: Add verification that number is representable?
    /// <summary>
    /// For use with the MatchNumber regex. Parses a hexadecimal floating point number from a match. Does not verify
    /// that the given number can be represented as a double-precision floating point value.
    /// </summary>
    /// <param name="match">Match to parse.</param>
    /// <returns>Double-precision floating point value of matched number.</returns>
    // Unfortunately, hexadecimal floats are uncommon enough that there aren't any C# standard library functions for
    // parsing them, so this has to be bespoke. It is parsed as such:
    // Significand: Convert to integer as usual.
    // Fraction: In order 1/16th, 1/256th, 1/4096th, etc. Always representable exactly in a float with enough precision.
    // Exponent: Power of 2 in decimal form, not hex.
    // Add the fraction to the significand, then multiply by 2 to the power of the exponent.
    private static double ParseHexFloat(Match match)
    {
        var significand = (double)Convert.ToInt64(match.Captures[1].Value, 16);
        
        var fraction = 0.0;
        var fractionString = match.Captures[2].Value;
        var placeValue = 1.0 / 16.0;
        for (var i = 1; i < fractionString.Length; i++)
        {
            fraction += HexLookupDict[fractionString[i]] * placeValue;
            placeValue *= 1.0 / 16.0;
        }

        var exponent = match.Captures[3].Length != 0
            ? Convert.ToDouble(match.Captures[3].Value[1..])
            : 0.0;

        return (significand + fraction) * Math.Pow(2.0, exponent);
    }
}