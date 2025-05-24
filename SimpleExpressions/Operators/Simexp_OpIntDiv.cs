using System.Reflection;
using System.Reflection.Emit;
using Ectoplasm.Runtime;
using Ectoplasm.Runtime.Values;

namespace Ectoplasm.SimpleExpressions.Operators;

public class Simexp_OpIntDiv : Simexp_Binary
{
    private static readonly MethodInfo SimpleOpInfo = typeof(LuaValue).GetMethod(nameof(LuaValue.SimpleIntDiv))!;
    
    /// <inheritdoc/>
    public override LuaValue Evaluate(LuaTable? env) => LuaValue.SimpleIntDiv(OpA!.Evaluate(env), OpB!.Evaluate(env));
    
    /// <inheritdoc/>
    public override void Compile(ILGenerator il)
    {
        OpA!.Compile(il);
        OpB!.Compile(il);
        il.Emit(OpCodes.Call, SimpleOpInfo);
    }
}