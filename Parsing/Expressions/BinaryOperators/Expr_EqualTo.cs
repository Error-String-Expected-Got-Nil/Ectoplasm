namespace Ectoplasm.Parsing.Expressions.BinaryOperators;

// Not equal to is implemented as inverting the result of equal to
public class Expr_EqualTo(bool invert, ushort line, ushort col) : Expr_Binary(line, col)
{
    public override string ToString() => base.ToString() + (invert ? " <inverted>" : "");
}