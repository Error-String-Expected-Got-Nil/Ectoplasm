namespace Ectoplasm.SimpleExpressions.Operators;

public abstract class Simexp_Unary : SimpleExpression
{
    protected SimpleExpression? Op;

    private bool _init;
    /// <inheritdoc/>
    public override bool IsInit => _init;

    /// <inheritdoc/>
    public override void Init(Stack<SimpleExpression> stack)
    {
        Op = stack.Pop();
        if (!Op.IsInit) Op.Init(stack);

        _init = true;
    }
}