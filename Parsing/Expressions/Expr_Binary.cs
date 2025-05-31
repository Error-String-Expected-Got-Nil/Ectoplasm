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

    public override IEnumerable<(Expression Expr, int Depth)> DepthFirstEnumerate(int depth = 0)
        => base.DepthFirstEnumerate(depth)
            .Concat(OpA!.DepthFirstEnumerate(depth + 1))
            .Concat(OpB!.DepthFirstEnumerate(depth + 1));
}