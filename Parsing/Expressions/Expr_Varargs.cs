using Ectoplasm.Runtime.Values;
using Ectoplasm.Utils;
using System.Reflection.Emit;

namespace Ectoplasm.Parsing.Expressions;

public class Expr_Varargs(ushort line, ushort col) : Expression(line, col)
{
    public override bool IsVariadic => true;

    public override void Compile(ILGenerator il, Prototype proto)
    {
        if (!proto.IsVararg) throw new InvalidOperationException("Attempt to compile vararg expression for " +
            "non-variadic function");

        // Vararg functions add an additional IL local variable slot beyond what is in their locals array, of type
        // LuaValue[]. This holds the variadic arguments for the call, if any. Standard compilation of this expression
        // (for a single value) takes the first of these values, or produces nil if there are no variadic arguments.

        il.LoadLocal((ushort)proto.Locals.Count);
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Ldlen);
        var nonzero = il.DefineLabel();
        var end = il.DefineLabel();
        il.Emit(OpCodes.Brtrue_S, nonzero); // List is non-empty, jump to push first element
        il.Emit(OpCodes.Pop); // List is empty, pop the extra ref on top, produce a nil, jump to end
        il.Emit(OpCodes.Call, ReflectionRefs.LuaValue_Default);
        il.Emit(OpCodes.Br, end);
        il.MarkLabel(nonzero);
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Ldelem); // Index first element from vararg list
        il.MarkLabel(end);
    }

    public override void CompileVariadic(ILGenerator il, Prototype proto)
    {
        il.Emit(OpCodes.Ldarg_1);
        il.LoadLocal((ushort)proto.Locals.Count);
        il.Emit(OpCodes.Call, ReflectionRefs.LuaState_AppendArray);
    }

    internal override void Initialize(Stack<Expression> stack) { }
}