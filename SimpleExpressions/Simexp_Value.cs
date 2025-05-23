using Ectoplasm.Runtime;
using Ectoplasm.Runtime.LuaValue;

namespace Ectoplasm.SimpleExpressions;

public class Simexp_Value(LuaValue value) : SimpleExpression
{
    public override bool IsInit => true;
    
    public override LuaValue Evaluate(LuaTable env) => value;

    public override void Init(Stack<SimpleExpression> stack) { }
}