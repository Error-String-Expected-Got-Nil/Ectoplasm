using System.Reflection.Emit;
using Ectoplasm.Runtime.Values;

namespace Ectoplasm.Parsing.Expressions.BinaryOperators;

// Not equal to is implemented as inverting the result of equal to
public class Expr_EqualTo(bool invert, ushort line, ushort col) : Expr_Binary(line, col)
{
    public override void Compile(ILGenerator il, Prototype proto)
    {
        il.Emit(OpCodes.Ldarg_1);
        OpA.Compile(il, proto);
        OpB.Compile(il, proto);
        il.Emit(invert ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Call, ReflectionRefs.Op_EqualTo);
    }

    public override string ToString() => base.ToString() + (invert ? " <inverted>" : "");
}