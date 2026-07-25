using System.Reflection.Emit;
using Ectoplasm.Runtime.Values;

namespace Ectoplasm.Parsing.Expressions.BinaryOperators;

public class Expr_Sub(ushort line, ushort col) : Expr_Binary(line, col)
{
    public override void Compile(ILGenerator il, Prototype proto)
    {
        il.Emit(OpCodes.Ldarg_1);
        OpA.Compile(il, proto);
        OpB.Compile(il, proto);
        il.Emit(OpCodes.Call, ReflectionRefs.Op_Sub);
    }
}