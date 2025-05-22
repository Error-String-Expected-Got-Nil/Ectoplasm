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
    public override string ToString() => $"{Type} [{StartLine}, {StartCol}]";
}