using Ectoplasm.Utils;

namespace Ectoplasm.Lexing;

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