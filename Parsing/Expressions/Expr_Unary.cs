namespace Ectoplasm.Parsing.Expressions;

public class Expr_Unary(ushort line, ushort col) : Expression(line, col)
{
    internal Expression? Op;
    
    internal override void Initialize(Stack<Expression> stack)
    {
        Op = stack.Pop();
        Op.Initialize(stack);
    }

    public override IEnumerable<(Expression Expr, int Depth)> DepthFirstEnumerate(int depth = 0)
        => base.DepthFirstEnumerate(depth)
            .Concat(Op!.DepthFirstEnumerate(depth + 1));
}