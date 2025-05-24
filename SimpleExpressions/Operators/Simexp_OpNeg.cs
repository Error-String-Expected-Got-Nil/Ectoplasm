using System.Reflection;
using System.Reflection.Emit;
using Ectoplasm.Runtime;
using Ectoplasm.Runtime.Values;

namespace Ectoplasm.SimpleExpressions.Operators;

public class Simexp_OpNeg : Simexp_Unary
{
    private static readonly MethodInfo SimpleOpInfo = typeof(LuaValue).GetMethod(nameof(LuaValue.SimpleNeg))!;
    
    /// <inheritdoc/>
    public override LuaValue Evaluate(LuaTable? env) => LuaValue.SimpleNeg(Op!.Evaluate(env));

    /// <inheritdoc/>
    public override void Compile(ILGenerator il)
    {
        Op!.Compile(il);
        il.Emit(OpCodes.Call, SimpleOpInfo);
    }
}