namespace Ectoplasm.Parsing.Expressions;

public class Expr_Unary(ushort line, ushort col) : Expression(line, col)
{
    internal Expression? Op;
    
    internal override void Initialize(Stack<Expression> stack)
    {
        Op = stack.Pop();
        Op.Initialize(stack);
    }
}