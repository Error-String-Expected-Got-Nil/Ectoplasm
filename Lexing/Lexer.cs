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
                // If we fail to match a sequence, that means the shortest token from the previous match must be the
                // token we actually found. This length will always be equal to offset, as offset equals the current
                // match length - 1.
                if (offset == 0)
                    throw new LuaLexingException($"Unrecognized symbol '{source[position]}'", line, col);
                
                var matchedToken 
                    = PrevMatches.First(match => match.Key.Length == offset).Value;
                
                return new LuaToken(source.AsMemory(position, offset), null, 
                    matchedToken, line, col, line, (ushort)(col + offset));
            }

            if (CurMatches.Count == 1)
                // If we match only a single token, that must be the correct token.
                return new LuaToken(source.AsMemory(position, offset + 1), null,
                    CurMatches[0].Value, line, col, line, (ushort)(col + offset + 1));
            
            // Otherwise, move contents of CurMatches to PrevMatches, and continue with the next character.
            PrevMatches.Clear();
            foreach (var item in CurMatches) PrevMatches.Add(item);
            CurMatches.Clear();
        }

        throw new LuaLexingException("Failed to read symbol", line, col);
    }

    private static LuaToken ReadName(string source, int position, ushort line, ushort col)
    {
        var match = Grammar.MatchName.Match(source, position);

        if (match.Length == 0) 
            throw new LuaLexingException("Failed to read name", line, col);

        if (Grammar.Keywords.TryGetValue(match.Value, out var keyword))
            return new LuaToken(source.AsMemory(position, match.Length), null, keyword, line, col,
                line, (ushort)(col + match.Length));

        return new LuaToken(source.AsMemory(position, match.Length), match.Value, TokenType.Name, line,
            col, line, (ushort)(col + match.Length));
    }

    private static LuaToken ReadNumber(string source, int position, ushort line, ushort col)
    {
        var match = Grammar.MatchNumber.Match(source, position);
        
        if (match.Length == 0) 
            throw new LuaLexingException("Failed to read number", line, col);

        if (Grammar.IsFloatMatch(match))
            return new LuaToken(source.AsMemory(position, match.Length),
                Grammar.ParseFloatMatch(match), TokenType.Numeral, line, col, line, 
                (ushort)(col + match.Length));

        return new LuaToken(source.AsMemory(position, match.Length),
            Grammar.ParseIntegerMatch(match), TokenType.Numeral, line, col, line, 
            (ushort)(col + match.Length));
    }
}