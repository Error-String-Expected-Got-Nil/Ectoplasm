using System.Reflection.Emit;
using Ectoplasm.Runtime;
using Ectoplasm.Runtime.Values;
using Ectoplasm.Utils;

namespace Ectoplasm.SimpleExpressions;

public class Simexp_Value(LuaValue value, int upvalueIndex) : SimpleExpression
{
    /// <inheritdoc/>
    public override bool IsInit => true;
    
    /// <inheritdoc/>
    public override LuaValue Evaluate(LuaTable? env) => value;

    /// <inheritdoc/>
    public override void Init(Stack<SimpleExpression> stack) { }

    /// <inheritdoc/>
    public override void Compile(ILGenerator il)
    {
        // Arg 0 contains upvalue array
        il.Emit(OpCodes.Ldarg_0);
        il.LoadConstant(upvalueIndex);
        il.Emit(OpCodes.Ldelem, typeof(LuaValue));
    }
}