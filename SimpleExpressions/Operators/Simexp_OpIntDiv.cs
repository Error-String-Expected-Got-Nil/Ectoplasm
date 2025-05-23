using Ectoplasm.Runtime;
using Ectoplasm.Runtime.LuaValue;

namespace Ectoplasm.SimpleExpressions.Operators;

public class Simexp_OpIntDiv : Simexp_Binary
{
    /// <inheritdoc/>
    public override LuaValue Evaluate(LuaTable env) => LuaValue.SimpleIntDiv(OpA!.Evaluate(env), OpB!.Evaluate(env));
}