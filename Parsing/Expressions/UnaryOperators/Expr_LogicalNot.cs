using System.Reflection.Emit;
using Ectoplasm.Runtime.Values;

namespace Ectoplasm.Parsing.Expressions.UnaryOperators;

public class Expr_LogicalNot(ushort line, ushort col) : Expr_Unary(line, col)
{
    public override void Compile(ILGenerator il, Prototype proto)
    {
        Op.Compile(il, proto);
        il.Emit(OpCodes.Call, ReflectionRefs.LuaValue_IsTruthy.GetMethod!);
        il.Emit(OpCodes.Not);
        il.Emit(OpCodes.Call, ReflectionRefs.LuaValue_NewBoolean);
    }
}