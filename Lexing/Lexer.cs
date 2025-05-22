using System.Text;

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

    private static LuaToken ReadWhitespace(string source, int position, ushort line, ushort col)
    {
        var match = Grammar.MatchWhitespace.Match(source, position);

        if (match.Length == 0)
            throw new LuaLexingException("Failed to read whitespace", line, col);

        // Only considers \n newlines as actual newlines. Also, note that lines and columns are 1-base indexed.
        var endLine = line;
        var endCol = col;
        foreach (var c in match.Value)
        {
            if (c == '\n')
            {
                endLine++;
                endCol = 1;
            }
            else
            {
                endCol++;
            }
        }

        return new LuaToken(source.AsMemory(position, match.Length), null, TokenType.Whitespace,
            line, col, endLine, endCol);
    }

    private static LuaToken ReadString(string source, int position, ushort line, ushort col)
    {
        var delimiter = source[position];
        if (delimiter is not ('\'' or '"'))
            throw new LuaLexingException("Failed to read string literal", line, col);

        var str = new StringBuilder();
        var escaped = false;
        var offset = 1;
        var endLine = line;
        var endCol = col;

        while (position + offset < source.Length)
        {
            var c = source[position + offset];

            if (escaped)
            {
                escaped = false;
                
                if (Grammar.SimpleEscapes.TryGetValue(c, out var escapeChar))
                {
                    str.Append(escapeChar);
                    offset++;
                    endCol++;
                    continue;
                }
                
                // Besides simple escape replacements, Lua also supports a few other more complex ones:
                
                // "Ignore next whitespace" escape, causes the next sequence of whitespace characters to be ignored.
                if (c == 'z')
                {
                    if (position + offset + 1 >= source.Length)
                        throw new LuaLexingException("Found end of source before completing string", line, col);

                    // If next character isn't whitespace, we don't have to do anything.
                    if (!Grammar.IsWhitespace(source[position + offset + 1]))
                    {
                        offset++;
                        endCol++;
                        continue;
                    }

                    // After verifying the next character *is* whitespace, we can just use the existing ReadWhitespace
                    // function to consume the sequence.
                    var token = ReadWhitespace(source, position + offset + 1, endLine, (ushort)(endCol + 1));
                    
                    offset += token.OriginalString.Length + 1;
                    endLine = token.EndLine;
                    endCol = token.EndCol;
                    continue;
                }

                // Hex byte literal. Always 3 characters long, counting the 'x'.
                if (c == 'x')
                {
                    if (position + offset + 2 >= source.Length)
                        throw new LuaLexingException("Found end of source before completing string", line, col);

                    var firstDigit = source[position + offset + 1];
                    var secondDigit = source[position + offset + 2];

                    if (!Grammar.IsHexDigit(firstDigit))
                        throw new LuaLexingException("Expected hexadecimal digit for hex byte escape sequence", endLine,
                            endCol);
                    if (!Grammar.IsHexDigit(secondDigit))
                        throw new LuaLexingException("Expected hexadecimal digit for hex byte escape sequence", endLine,
                            (ushort)(endCol + 1));

                    var hexValue = Grammar.HexDigitValue[firstDigit] * 16 + Grammar.HexDigitValue[secondDigit];
                    
                    str.Append((char)hexValue);
                    offset += 3;
                    endCol += 3;
                    continue;
                }

                // Decimal byte literal
                if (Grammar.IsDigit(c))
                {
                    var decMatch = Grammar.MatchDecByteEscape.Match(source, position + offset);

                    var decValue = int.Parse(decMatch.Value);
                    if (decValue > byte.MaxValue)
                        throw new LuaLexingException("Decimal byte escape represented value greater than the maximum " +
                                                     "value of a byte", endLine, endCol);

                    str.Append((char)decValue);
                    offset += decMatch.Length;
                    endCol += (ushort)decMatch.Length;
                    continue;
                }

                // Unicode code point escape
                if (c == 'u')
                {
                    var unicodeMatch = Grammar.MatchUnicodeEscape.Match(source, position + offset + 1);

                    if (unicodeMatch.Length == 0)
                        throw new LuaLexingException("Expected curly-bracketed Unicode hex code point", endLine, 
                            endCol);

                    var codepoint = Convert.ToUInt32(unicodeMatch.Groups[0].Value, 16);

                    string utf16String;
                    try
                    {
                        utf16String = char.ConvertFromUtf32((int)codepoint);
                    }
                    catch (ArgumentException)
                    {
                        throw new LuaLexingException(
                            "Failed to parse Unicode code point escape, sequence did not represent a valid code " +
                            "point. While the standard Lua specification permits invalid code points, for " +
                            "implementation reasons Ectoplasm does not. Code points greater than U+10FFFF and " +
                            "between U+D800 and U+DFFF (inclusive) are invalid.", endLine, endCol);
                    }

                    str.Append(utf16String);
                    offset += unicodeMatch.Length + 1;
                    endCol += (ushort)(unicodeMatch.Length + 1);
                    continue;
                }

                // Escape followed by a literal newline will allow the string to continue across the newline, and add
                // a newline to the string.
                var newlineMatch = Grammar.MatchNewline.Match(source, position + offset);
                if (newlineMatch.Length != 0)
                {
                    str.Append('\n');
                    offset += newlineMatch.Length + 1;
                    endLine++;
                    endCol = 1;
                    continue;
                }
                
                // Otherwise, this was an unrecognized escape sequence.
                throw new LuaLexingException($"Unrecognized escape character '{c}'", endLine, endCol);
            }
            
            if (c == Grammar.Escape)
            {
                escaped = true;
                offset++;
                endCol++;
                continue;
            }

            if (c == delimiter)
                return new LuaToken(source.AsMemory(position, offset + 1), str.ToString(),
                    TokenType.String, line, col, endLine, endCol);

            if (c is '\n' or '\r')
                throw new LuaLexingException("Unfinished string literal", endLine, endCol);
            
            str.Append(c);
            offset++;
            endCol++;
        }

        throw new LuaLexingException("Found end of source before completing string", line, col);
    }

    private static LuaToken ReadLongString(string source, int position, ushort line, ushort col)
    {
        if (source[position] != '[')
            throw new LuaLexingException("Failed to read long string literal", line, col);

        char c;
        var offset = 1;
        var endLine = line;
        var endCol = col;
        var level = 0;
        
        // Read the opening long bracket and determine its level
        while (true)
        {
            if (position + offset >= source.Length)
                throw new LuaLexingException("Found end of source before completing long string", line, col);

            c = source[position + offset];
            
            if (c == '=')
            {
                level++;
            }
            else if (c == '[')
            {
                offset++;
                endCol++;
                break;
            }
            else
            {
                // Special case: If there was no level, this is probably just not a long string, rather than an invalid
                // long string. However, it's definitely an invalid long string if there was any level.
                if (level == 0) return default;
                throw new LuaLexingException($"Unexpected character '{c}' in opening long bracket of long string",
                    endLine, endCol);
            }

            offset++;
            endCol++;
        }

        var str = new StringBuilder();

        // Attempt to consume a newline immediately after the opening bracket, and discard it if there was one.
        var openingNewlineMatch = Grammar.MatchNewline.Match(source, position + offset);
        if (openingNewlineMatch.Length != 0)
        {
            offset += openingNewlineMatch.Length;
            endLine++;
            endCol = 1;
        }

        while (true)
        {
            if (position + offset >= source.Length)
                throw new LuaLexingException("Found end of source before completing long string", line, col);

            c = source[position + offset];

            // Match closing long bracket
            if (c == ']')
            {
                str.Append(']');
                offset++;
                endCol++;
                
                var matchLevel = 0;
                while (true)
                {
                    if (position + offset >= source.Length)
                        throw new LuaLexingException("Found end of source before completing long string", line, col);

                    c = source[position + offset];
                    
                    if (c == '=')
                    {
                        matchLevel++;
                        str.Append('=');
                        offset++;
                        endCol++;
                        continue;
                    }

                    if (c == ']')
                    {
                        // If this closing long bracket didn't have the same level as the opening long bracket, we can
                        // just fall through to appending; it was a normal part of the string.
                        if (matchLevel != level) break;
                        
                        // Otherwise, remove previous level + 1 characters (all part of closing long bracket) and return
                        // the token, as the string is finished
                        str.Remove(str.Length - level - 1, level + 1);
                        offset++;
                        endCol++;
                        return new LuaToken(source.AsMemory(position, offset + 1),
                            str.ToString(), TokenType.String, line, col, endLine, endCol);
                    }
                    
                    // Character wasn't ] or =, not part of closing long bracket, fall through to append.
                    break;
                }
            }
            
            // Truncate all newline sequences to just \n
            if (c is '\n' or '\r')
            {
                var newlineMatch = Grammar.MatchNewline.Match(source, position + offset);
                str.Append('\n');
                offset += newlineMatch.Length;
                endLine++;
                endCol = 1;
                continue;
            }

            // Otherwise, we append the literal character
            str.Append(c);
            offset++;
            endCol++;
        }
    }
}