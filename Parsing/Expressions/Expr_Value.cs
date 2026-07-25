using System.Reflection.Emit;
using Ectoplasm.Runtime;
using Ectoplasm.Runtime.Stdlib;
using Ectoplasm.Runtime.Values;

namespace Ectoplasm.Parsing.Expressions;

public class Expr_Value(LuaValue value, ushort line, ushort col) : Expression(line, col)
{
    public override void Compile(ILGenerator il, Prototype proto)
    {
        switch (value._kind)
        {
            case LuaValueKind.Nil:
                il.Emit(OpCodes.Call, ReflectionRefs.LuaValue_Default);
                break;
            case LuaValueKind.Boolean:
                il.Emit(value._boolean ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Call, ReflectionRefs.LuaValue_NewBoolean);
                break;
            case LuaValueKind.Integer:
                il.Emit(OpCodes.Ldc_I8, value._integer);
                il.Emit(OpCodes.Call, ReflectionRefs.LuaValue_NewInteger);
                break;
            case LuaValueKind.Float:
                il.Emit(OpCodes.Ldc_R8, value._float);
                il.Emit(OpCodes.Call, ReflectionRefs.LuaValue_NewFloat);
                break;
            case LuaValueKind.String:
                il.Emit(OpCodes.Ldstr, (string)value._ref);
                il.Emit(OpCodes.Call, ReflectionRefs.LuaValue_NewString);
                break;
            default:
                throw new InvalidOperationException("Attempt to compile constant Lua value of non-primitive type");
        }
    }

    internal override void Initialize(Stack<Expression> stack) { }

    public override string ToString() 
        => base.ToString() + $" <{GlobalFunctions.LuaToString(value, true)}>";
}