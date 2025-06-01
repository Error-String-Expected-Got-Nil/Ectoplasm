namespace Ectoplasm.Parsing.Statements;

public abstract class Statement(ushort line, ushort col)
{
    public ushort StartLine => line;
    public ushort StartCol => col;

    // TODO
}