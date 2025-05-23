using Ectoplasm.Runtime;
using Ectoplasm.Runtime.Values;
using LuaValue = Ectoplasm.Runtime.Values.LuaValue;

namespace Ectoplasm.SimpleExpressions.Operators;

public class Simexp_OpDiv : Simexp_Binary
{
    /// <inheritdoc/>
    public override LuaValue Evaluate(LuaTable env) => LuaValue.SimpleDiv(OpA!.Evaluate(env), OpB!.Evaluate(env));
}