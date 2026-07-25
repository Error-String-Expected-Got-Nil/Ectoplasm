namespace Ectoplasm.Parsing.Expressions;

public class Expr_Varargs(ushort line, ushort col) : Expression(line, col)
{
    public override bool IsVariadic => true;
    
    internal override void Initialize(Stack<Expression> stack) { }
}