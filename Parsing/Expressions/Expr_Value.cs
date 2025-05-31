using Ectoplasm.Runtime.Stdlib;
using Ectoplasm.Runtime.Values;

namespace Ectoplasm.Parsing.Expressions;

public class Expr_Value(LuaValue value, ushort line, ushort col) : Expression(line, col)
{
    internal override void Initialize(Stack<Expression> stack) { }

    public override string ToString() 
        => base.ToString() + $" <{GlobalFunctions.LuaToStringUtf16(value, true)}>";
}