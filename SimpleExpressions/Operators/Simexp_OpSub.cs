using System.Reflection;
using System.Reflection.Emit;
using Ectoplasm.Runtime.Values;
using Ectoplasm.Runtime.Tables;

namespace Ectoplasm.SimpleExpressions.Operators;

public class Simexp_OpSub : Simexp_Binary
{
    private static readonly MethodInfo SimpleOpInfo = typeof(LuaValue).GetMethod(nameof(LuaValue.SimpleSub))!;
    
    /// <inheritdoc/>
    public override LuaValue Evaluate(LuaTable? env) => LuaValue.SimpleSub(OpA!.Evaluate(env), OpB!.Evaluate(env));
    
    /// <inheritdoc/>
    public override void Compile(ILGenerator il)
    {
        OpA!.Compile(il);
        OpB!.Compile(il);
        il.Emit(OpCodes.Call, SimpleOpInfo);
    }
}