using Ectoplasm.Runtime;
using Ectoplasm.Runtime.Values;

namespace Ectoplasm.SimpleExpressions.Operators;

public abstract class Simexp_Binary : SimpleExpression
{
    protected SimpleExpression? OpA;
    protected SimpleExpression? OpB;
    
    private bool _init;
    /// <inheritdoc/>
    public override bool IsInit => _init;

    /// <inheritdoc/>
    public override void Init(Stack<SimpleExpression> stack)
    {
        OpB = stack.Pop();
        if (!OpB.IsInit) OpB.Init(stack);

        OpA = stack.Pop();
        if (!OpA.IsInit) OpA.Init(stack);

        _init = true;
    }
}