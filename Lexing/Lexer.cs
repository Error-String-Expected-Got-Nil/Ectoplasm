using Ectoplasm.Lexing.Tokens;

namespace Ectoplasm.Lexing;

public static class Lexer
{
    public static IEnumerable<LuaToken> Lex(string source)
    {
        yield break;
    }

    // All 'position' parameters of the reader functions are the index of the first character to start reading from.
    // 'line' is the line that character starts on (indexed from 0), and 'col' is the column in that line (also from 0).
    // 'source' is of course the source string.
    
    private static LuaToken ReadSymbol(string source, int position, ushort line, ushort col)
    {
        Span<char> buffer = stackalloc char[Grammar.MaxSymbolLength];
        
        for (var offset = 0; offset < Grammar.MaxSymbolLength; offset++)
        {
            buffer[offset] = source[position + offset];
            // TODO: Check Symbols trie prefixes
        }

        return default;
    }
}