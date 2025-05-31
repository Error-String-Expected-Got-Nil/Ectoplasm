namespace Ectoplasm.Parsing.Expressions;

public class Expr_Table(List<(Expression Key, Expression Value)> keyed, List<Expression> unkeyed,
    ushort line, ushort col) : Expression(line, col)
{
    internal override void Initialize(Stack<Expression> stack) { }
}