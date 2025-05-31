namespace Ectoplasm.Parsing.Expressions;

public class Expr_Variable(string name, ushort line, ushort col) : Expression(line, col)
{
    internal override void Initialize(Stack<Expression> stack) { }

    public override string ToString() => base.ToString() + $" <{name}>";
}