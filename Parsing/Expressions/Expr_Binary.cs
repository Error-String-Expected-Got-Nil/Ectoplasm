namespace Ectoplasm.Parsing.Expressions;

public abstract class Expr_Binary(ushort line, ushort col) : Expression(line, col)
{
    internal Expression? OpA;
    internal Expression? OpB;

    internal override void Initialize(Stack<Expression> stack)
    {
        OpB = stack.Pop();
        OpB.Initialize(stack);

        OpA = stack.Pop();
        OpA.Initialize(stack);
    }
}