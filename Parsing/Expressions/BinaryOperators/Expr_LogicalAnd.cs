using System.Reflection.Emit;
using Ectoplasm.Runtime.Values;

namespace Ectoplasm.Parsing.Expressions.BinaryOperators;

public class Expr_LogicalAnd(ushort line, ushort col) : Expr_Binary(line, col)
{
    public override void Compile(ILGenerator il, Prototype proto)
    {
        OpA.Compile(il, proto); // Evaluate first operand
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Call, ReflectionRefs.LuaValue_IsTruthy.GetMethod!); // Check truthiness
        var shortcut = il.DefineLabel();
        il.Emit(OpCodes.Brfalse, shortcut); // First operand false, shortcut and don't evaluate second
        il.Emit(OpCodes.Pop); // First was duplicated to return if false, but it wasn't, need to pop
        OpB.Compile(il, proto); // Evaluate second to return it
        il.MarkLabel(shortcut);
    }
}