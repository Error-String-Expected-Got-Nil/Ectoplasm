namespace Ectoplasm.Parsing.Expressions.BinaryOperators;

// Greater than is implemented as swapping the operands of less than
public class Expr_LessThan(bool swapOperands, ushort line, ushort col) : Expr_Binary(line, col)
{
    internal override void Initialize(Stack<Expression> stack)
    {
        base.Initialize(stack);

        if (swapOperands) (OpA, OpB) = (OpB, OpA);
    }
}