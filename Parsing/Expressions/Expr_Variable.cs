namespace Ectoplasm.Parsing.Expressions;

public class Expr_Variable(string name, ushort line, ushort col) : Expression(line, col)
{
    public string Name => name;

    public override bool IsAssignable => true;

    internal override void Initialize(Stack<Expression> stack) { }

    public override string ToString() => base.ToString() + $" <{name}>";
}