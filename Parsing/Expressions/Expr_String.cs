using Ectoplasm.Utils;

namespace Ectoplasm.Parsing.Expressions;

public class Expr_String(string value, ushort line, ushort col) : Expression(line, col)
{
    internal override void Initialize(Stack<Expression> stack) { }

    public override string ToString() => base.ToString() + $" <{value.GetEscapedString()}>";
}