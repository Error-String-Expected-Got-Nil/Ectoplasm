using Ectoplasm.Runtime;
using Ectoplasm.Runtime.LuaValue;

namespace Ectoplasm.SimpleExpressions;

public class Simexp_Variable(LuaValue index) : SimpleExpression
{
    public override bool IsInit => true;

    public override LuaValue Evaluate(LuaTable env) => env[index];

    public override void Init(Stack<SimpleExpression> stack) { }
}