using System.Reflection;
using System.Reflection.Emit;
using Ectoplasm.Runtime.Values;
using Ectoplasm.Runtime.Tables;

namespace Ectoplasm.SimpleExpressions.Operators;

public class Simexp_OpConcat : Simexp_Binary
{
    private static readonly MethodInfo SimpleOpInfo = typeof(LuaValue).GetMethod(nameof(LuaValue.SimpleConcat))!;
    
    /// <inheritdoc/>
    public override LuaValue Evaluate(LuaTable? env) => LuaValue.SimpleConcat(OpA!.Evaluate(env), OpB!.Evaluate(env));
    
    /// <inheritdoc/>
    public override void Compile(ILGenerator il)
    {
        OpA!.Compile(il);
        OpB!.Compile(il);
        il.Emit(OpCodes.Call, SimpleOpInfo);
    }
}