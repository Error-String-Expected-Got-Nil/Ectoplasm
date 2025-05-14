using System.Text.RegularExpressions;
using Ectoplasm.Lexing.Tokens;
using KTrie;

using static Ectoplasm.Lexing.Tokens.TokenType;

namespace Ectoplasm.Lexing;

/// <summary>
/// Static information for matching tokens from raw code during lexing.
/// </summary>
public static partial class Grammars
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
    /// Matches both integer and floating point decimal and hex numbers, all valid in Lua. The first capture will
    /// contain the hex identifier (0x or 0X), the next three will be the significand, fraction, and exponent for a
    /// hex number, and the next three after that will be significand, fraction, and exponent for a decimal number. The
    /// significand will be only digits, the fraction will have a leading period, and the exponent will have a leading
    /// e/E/p/P and possibly sign.
    /// </summary>
    [GeneratedRegex(@"(0[xX])([\da-fA-F]+)(\.[\da-fA-F]+)?([pP][+-]?[\da-fA-F]+)?|(\d+)(\.\d+)?([eE][+-]?\d+)?")]
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
}