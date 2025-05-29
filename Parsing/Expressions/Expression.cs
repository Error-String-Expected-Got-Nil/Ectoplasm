namespace Ectoplasm.Parsing.Expressions;

public abstract class Expression(ushort line, ushort col)
{
    public ushort StartLine { get; private set; } = line;
    public ushort StartCol { get; private set; } = col;

    // TODO
}