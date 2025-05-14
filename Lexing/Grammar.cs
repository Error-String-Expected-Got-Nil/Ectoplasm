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
    /// Matches both integer and floating point decimal and hex numbers, all valid in Lua. Groups:
    /// 1: Hex identifier 0x or 0X if present. If empty, number is not hexadecimal.
    /// 2 through 4: Empty if not hex.
    /// 2: Hex significand.
    /// 3: Hex fraction, if present, with leading period.
    /// 4: Decimal exponent (power of 2), if present, with leading p or P and possibly a sign following it.
    /// 5 through 7: Empty if hex.
    /// 5: Decimal significand.
    /// 6: Decimal fraction, if present, with leading period.
    /// 7: Decimal exponent (power of 10), if present, with leading e or E and possibly a sign following it.
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
    public static bool IsHexMatch(Match match) => match.Groups[1].Length != 0;

    /// <summary>
    /// For use with the MatchNumber regex. Checks if match represents a float or an integer.
    /// </summary>
    /// <param name="match"></param>
    /// <returns></returns>
    public static bool IsFloatMatch(Match match)
        => IsHexMatch(match)
            ? match.Groups[3].Length != 0 || match.Groups[4].Length != 0
            : match.Groups[6].Length != 0 || match.Groups[7].Length != 0;

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
        => Convert.ToInt64(match.Groups[5].Value, 10);

    /// <summary>
    /// For use with the MatchNumber regex. Parses a decimal floating point number from a match.
    /// </summary>
    /// <param name="match">Match to parse.</param>
    /// <returns>Double-precision floating point value of matched number.</returns>
    private static double ParseDecimalFloat(Match match)
        => double.Parse(match.Groups[5].Value + match.Groups[6].Value + match.Groups[7].Value);

    /// <summary>
    /// For use with the MatchNumber regex. Parses a hexadecimal integer number from a match.
    /// </summary>
    /// <param name="match">Match to parse.</param>
    /// <returns>64-bit integer value of matched number.</returns>
    private static long ParseHexInteger(Match match)
        => Convert.ToInt64(match.Groups[2].Value, 16);
    
    /// <summary>
    /// For use with the MatchNumber regex. Parses a hexadecimal floating point number from a match. Does not verify
    /// that the given number can be represented as a double-precision floating point value.
    /// </summary>
    /// <param name="match">Match to parse.</param>
    /// <returns>Double-precision floating point value of matched number.</returns>
    /// <remarks>
    /// This function is not fully checked; some un-representable doubles may pass through (likely as +/-inf), others
    /// will be caught and result in an exception.
    /// </remarks>
    // Unfortunately, hexadecimal floats are uncommon enough that there aren't any C# standard library functions for
    // parsing them, so this has to be bespoke. Fortunately, the nature of the format means it's actually relatively
    // easy to parse precisely. This relies heavily on how IEEE floating point numbers are represented as bits.
    // 1: Take the fractional part, minus the leading period, and convert it to an integer.
    // 1a: If this is >= 1 << 52, there is not enough precision to represent it.
    // 2: Bitshift it left by (52 - 4 * fraction length) bits (this is to add 0s to the end of the fraction bits to fill
    //     the significand).
    // 3: Bitwise OR this with 0x3ff0000000000000 to set the exponent bits to make the exponent 0.
    // 4: Bit-cast this to a double.
    // 5: Convert the integer part to a double, subtract 1.0, and add it to the bit-casted double (this is because IEEE
    //     floating point format already implicitly adds 1.0 to the significand of normalized numbers).
    // 6: Multiply this by 2.0 to the power of the exponent part.
    //
    // It's difficult to describe *why* exactly this works, but it does. The best reference I can point to is simply
    // to read up on how IEEE floating point binary format works.
    //
    // Additionally: If there is no fractional part, we don't need to bother with this mess, and can simply take the
    // integer part as a double and multiply by 2 to the power of the exponent part.
    private static double ParseHexFloat(Match match)
    {
        // Simple case: No fractional part.
        if (match.Groups[3].Length == 0)
            return Convert.ToInt64(match.Groups[2].Value, 16)
                   * Math.Pow(2.0, match.Groups[4].Length != 0 
                       ? Convert.ToInt64(match.Groups[4].Value[1..], 10)
                       : 0.0);

        // Step 1
        var fractionString = match.Groups[3].Value[1..];
        var fractionBits = Convert.ToInt64(fractionString, 16);
        if (fractionBits >= 1L << 52)
            throw new ArgumentException("Matched number has fraction too precise to represent as double.");
        
        // Step 2, 3, 4
        var fraction 
            = BitConverter.Int64BitsToDouble((fractionBits << 52 - 4 * fractionString.Length) | 0x3ff0000000000000);
        
        // Step 5
        var value = fraction + double.Parse(match.Groups[2].Value) - 1.0;
        
        // Step 6
        return value 
               * Math.Pow(2.0, match.Groups[4].Length != 0 
                   ? Convert.ToInt64(match.Groups[4].Value[1..], 10) 
                   : 0.0);
    }
}