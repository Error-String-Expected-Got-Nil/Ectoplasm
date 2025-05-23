using Ectoplasm.Runtime;
using Ectoplasm.Runtime.Values;
using LuaValue = Ectoplasm.Runtime.Values.LuaValue;

namespace Ectoplasm.SimpleExpressions.Operators;

public class Simexp_OpSub : Simexp_Binary
{
    /// <inheritdoc/>
    public override LuaValue Evaluate(LuaTable env) => LuaValue.SimpleSub(OpA!.Evaluate(env), OpB!.Evaluate(env));
}