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

    // These are used by the ReadSymbol function as buffers. Recycled for each call by clearing.
    private static readonly List<KeyValuePair<string, TokenType>> PrevMatches = new(Grammar.MaxPrefixCount);
    private static readonly List<KeyValuePair<string, TokenType>> CurMatches = new(Grammar.MaxPrefixCount);
    
    // TODO: There's probably a better way to do this.
    // TODO: After symbol matching, check if matched symbol was a quote mark or open bracket to try string matching
    private static LuaToken ReadSymbol(string source, int position, ushort line, ushort col)
    {
        Span<char> buffer = stackalloc char[Grammar.MaxSymbolLength];
        PrevMatches.Clear();
        CurMatches.Clear();
        
        for (var offset = 0; offset < Grammar.MaxSymbolLength; offset++)
        {
            buffer[offset] = source[position + offset];
            var prefixed = Grammar.Symbols.StartsWith(buffer[..offset]);
            CurMatches.AddRange(prefixed);

            if (CurMatches.Count == 0)
            {
                // If we fail to match a sequence, that means the longest token from the previous match must be the
                // token we actually found.
                if (PrevMatches.Count == 0)
                    throw new LexingException("Unrecognized symbol", line, col);
                
                var matchedToken = PrevMatches.MinBy(match => match.Key.Length).Value;
                return new LuaToken(source.AsMemory(position, offset + 1), null, 
                    matchedToken, line, col);
            }

            if (CurMatches.Count == 1)
                // If we match only a single token, that must be the correct token.
                return new LuaToken(source.AsMemory(position, offset + 1), null,
                    CurMatches[0].Value, line, col);
            
            // Otherwise, move contents of CurMatches to PrevMatches, and continue with the next character.
            PrevMatches.Clear();
            foreach (var item in CurMatches) PrevMatches.Add(item);
            CurMatches.Clear();
        }

        throw new LexingException("Failed to read symbol", line, col);
    }
}