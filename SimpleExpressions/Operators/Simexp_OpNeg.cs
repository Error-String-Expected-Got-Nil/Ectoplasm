using Ectoplasm.Runtime;
using Ectoplasm.Runtime.Values;

namespace Ectoplasm.SimpleExpressions.Operators;

public class Simexp_OpNeg : Simexp_Unary
{
    public override LuaValue Evaluate(LuaTable? env) => LuaValue.SimpleNeg(Op!.Evaluate(env));
}