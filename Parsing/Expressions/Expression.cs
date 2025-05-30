namespace Ectoplasm.Parsing.Expressions;

public abstract class Expression(ushort line, ushort col)
{
    /// <summary>
    /// The line of source this expression starts on.
    /// </summary>
    public ushort StartLine { get; private set; } = line;
    
    /// <summary>
    /// The column of source this expression starts on.
    /// </summary>
    public ushort StartCol { get; private set; } = col;

    /// <summary>
    /// Initialize this expression by popping operands from a given stack and initializing them.
    /// </summary>
    internal abstract void Initialize(Stack<Expression> stack);

    // TODO
}