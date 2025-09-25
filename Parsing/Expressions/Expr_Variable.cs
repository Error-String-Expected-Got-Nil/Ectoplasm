namespace Ectoplasm.Parsing.Expressions;

public class Expr_Variable(string name, ushort line, ushort col) : Expression(line, col)
{
    public string Name => name;
    
    internal override void Initialize(Stack<Expression> stack) { }

    public override string ToString() => base.ToString() + $" <{name}>";
}