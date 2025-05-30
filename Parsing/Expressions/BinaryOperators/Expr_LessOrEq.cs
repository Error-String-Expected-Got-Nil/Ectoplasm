namespace Ectoplasm.Parsing.Expressions.BinaryOperators;

// Greater than or equal to is implemented as swapping the operands of less than or equal to
public class Expr_LessOrEq(bool swapOperands, ushort line, ushort col) : Expr_Binary(line, col)
{
    internal override void Initialize(Stack<Expression> stack)
    {
        base.Initialize(stack);

        if (swapOperands) (OpA, OpB) = (OpB, OpA);
    }
}