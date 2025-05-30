using Ectoplasm.Utils;

namespace Ectoplasm.Lexing;

/// <summary>
/// A readonly record struct containing information about a single token from a Lua program, including the section of
/// source code that produced it.
/// </summary>
/// <param name="OriginalString">The slice of the original source string that produced this token.</param>
/// <param name="Data">
/// Extra data about this token. Only used by the following <see cref="TokenType"/>s: <br/>
/// - <see cref="TokenType.Name"/>: <see cref="string"/> containing the text of the name. <br/>
/// - <see cref="TokenType.String"/>: <see cref="string"/> containing the parsed contents of the string. <br/>
/// - <see cref="TokenType.Numeral"/>: Either a <see cref="long"/> or <see cref="double"/>, representing the parsed
/// value of the numeral.
/// </param>
/// <param name="Type">The <see cref="TokenType"/> of this token.</param>
/// <param name="StartLine">The line of source this token starts on.</param>
/// <param name="StartCol">The column of the line this token starts on that the token starts.</param>
/// <param name="EndLine">The line of source this token ends on.</param>
/// <param name="EndCol">The column of the line this token ends on that the token ends.</param>
public readonly record struct LuaToken(
    ReadOnlyMemory<char> OriginalString,
    object? Data,
    TokenType Type,
    ushort StartLine,
    ushort StartCol,
    ushort EndLine,
    ushort EndCol
)
{
    /// <summary>
    /// Gets the original string of this token, or the text "&lt;string&gt;" if this is a string token, since the
    /// original text of a string is often longer than desired for debug printouts.
    /// </summary>
    public string OriginalOrPlaceholder => Type is TokenType.String ? "<string>" : OriginalString.ToString();
    
    public override string ToString()
    {
        var formatted = FormattedData();
        return $"{formatted.Type} {formatted.Location} {formatted.Data}";
    }

    public (string Type, string Location, string Data) FormattedData()
        => (Type.ToString(), $"[{StartLine}, {StartCol}]", Type switch
        {
            TokenType.Name => $"<{(string)Data!}>",
            TokenType.Numeral => $"<{(Data! is long ? (long)Data : (double)Data!)}>",
            TokenType.String => $"<\"{((string)Data!).GetEscapedString()}\">",
            _ => ""
        });
}