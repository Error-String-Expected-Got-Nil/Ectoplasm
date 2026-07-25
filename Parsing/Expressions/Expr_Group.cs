using System.Reflection.Emit;

namespace Ectoplasm.Parsing.Expressions;

public class Expr_Group(ushort line, ushort col) : Expr_Unary(line, col)
{
    public override void Compile(ILGenerator il, Prototype proto)
    {
        // No extra work is needed here since this already always produces exactly one result.
        Op.Compile(il, proto);
    }
}