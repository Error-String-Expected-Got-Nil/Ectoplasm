namespace Ectoplasm.Lexing.Tokens;

public readonly record struct LuaToken(
    TokenType Type = TokenType.None,
    ushort StartLine = 0,
    ushort StartCol = 0,
    ushort EndLine = 0,
    ushort EndCol = 0,
    Memory<char> OriginalString = default
)
{
    public override string ToString() => $"{Type} [{StartLine}, {StartCol}]";
}