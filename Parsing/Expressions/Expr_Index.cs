using System.Reflection.Emit;
using Ectoplasm.Runtime.Values;

namespace Ectoplasm.Parsing.Expressions;

public class Expr_Index(ushort line, ushort col) : Expr_Binary(line, col)
{
    // OpA is table to index from
    // OpB is the key to index with

    public override bool IsAssignable => true;

    public override void Compile(ILGenerator il, Prototype proto)
    {
        il.Emit(OpCodes.Ldarg_1);
        OpA.Compile(il, proto);
        OpB.Compile(il, proto);
        il.Emit(OpCodes.Call, ReflectionRefs.Op_GetIndex);
    }

    public override void CompileAssign(ILGenerator il, Prototype proto, Action valueProducer)
    {
        il.Emit(OpCodes.Ldarg_1);
        OpA.Compile(il, proto);
        OpB.Compile(il, proto);
        valueProducer();
        il.Emit(OpCodes.Call, ReflectionRefs.Op_SetIndex);
    }
}