namespace Ectoplasm.Parsing.Expressions;

public class Expr_Varargs(ushort line, ushort col) : Expression(line, col)
{
    internal override void Initialize(Stack<Expression> stack) { }
}