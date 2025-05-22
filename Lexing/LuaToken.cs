namespace Ectoplasm.Lexing;

public readonly record struct LuaToken(
    ReadOnlyMemory<char> OriginalString = default,
    object? Data = null,
    TokenType Type = TokenType.None,
    ushort StartLine = 0,
    ushort StartCol = 0
)
{
    public override string ToString() => $"{Type} [{StartLine}, {StartCol}]";
}