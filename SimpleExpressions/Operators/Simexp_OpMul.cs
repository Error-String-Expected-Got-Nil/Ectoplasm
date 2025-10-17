using System.Reflection;
using System.Reflection.Emit;
using Ectoplasm.Runtime.Values;
using Ectoplasm.Runtime.Tables;

namespace Ectoplasm.SimpleExpressions.Operators;

public class Simexp_OpMul : Simexp_Binary
{
    private static readonly MethodInfo SimpleOpInfo = typeof(LuaValue).GetMethod(nameof(LuaValue.SimpleMul))!;
    
    /// <inheritdoc/>
    public override LuaValue Evaluate(LuaTable? env) => LuaValue.SimpleMul(OpA!.Evaluate(env), OpB!.Evaluate(env));
    
    /// <inheritdoc/>
    public override void Compile(ILGenerator il)
    {
        OpA!.Compile(il);
        OpB!.Compile(il);
        il.Emit(OpCodes.Call, SimpleOpInfo);
    }
}