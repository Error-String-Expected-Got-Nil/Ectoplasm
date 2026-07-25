using System.Reflection.Emit;
using Ectoplasm.Runtime.Values;

namespace Ectoplasm.Parsing.Expressions.UnaryOperators;

public class Expr_Neg(ushort line, ushort col) : Expr_Unary(line, col)
{
    public override void Compile(ILGenerator il, Prototype proto)
    {
        il.Emit(OpCodes.Ldarg_1);
        Op.Compile(il, proto);
        il.Emit(OpCodes.Call, ReflectionRefs.Op_Neg);
    }
}