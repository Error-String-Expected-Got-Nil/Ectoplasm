using System.Reflection.Emit;
using Ectoplasm.Runtime;
using Ectoplasm.Runtime.Values;
using LuaValue = Ectoplasm.Runtime.Values.LuaValue;

namespace Ectoplasm.SimpleExpressions;

public abstract class SimpleExpression
{
    /// <summary>
    /// Indicates if this expression is initialized already.
    /// </summary>
    public abstract bool IsInit { get; }
    
    /// <summary>
    /// Evaluate this expression, resolving its value.
    /// </summary>
    /// <param name="env">Table containing variables for the expression.</param>
    /// <returns>Value of the expression after computation.</returns>
    public abstract LuaValue Evaluate(LuaTable? env);
    
    /// <summary>
    /// Initialize this expression using operands from a stack. If uninitialized operands are encountered, they will be
    /// initialized from the same stack recursively. This can initialize an entire prefix-form expression from a stack
    /// with this call.
    /// </summary>
    public abstract void Init(Stack<SimpleExpression> stack);

    /// <summary>
    /// Takes an <see cref="ILGenerator"/> and uses it to emit instructions such that the stack transitions to put the
    /// result of evaluating this expression on top.
    /// </summary>
    public abstract void Compile(ILGenerator il);
}