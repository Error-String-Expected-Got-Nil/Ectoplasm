using Ectoplasm.Runtime;
using Ectoplasm.Runtime.Values;

namespace Ectoplasm.SimpleExpressions.Operators;

public class Simexp_OpConcat : Simexp_Binary
{
    public override LuaValue Evaluate(LuaTable? env) => LuaValue.SimpleConcat(OpA!.Evaluate(env), OpB!.Evaluate(env));
}