namespace Ectoplasm.Parsing.Expressions;

public class Expr_Table(
    List<(Expression Key, Expression Value)> keyed,
    List<Expression> unkeyed,
    ushort line,
    ushort col) : Expression(line, col)
{
    internal override void Initialize(Stack<Expression> stack) { }

    public override IEnumerable<(Expression Expr, int Depth)> DepthFirstEnumerate(int depth = 0)
        => base.DepthFirstEnumerate(depth)
            .Concat(keyed
                .Aggregate(new List<(Expression, int)>(),
                    (accum, item) =>
                    {
                        accum.AddRange(item.Key.DepthFirstEnumerate(depth + 1));
                        accum.AddRange(item.Value.DepthFirstEnumerate(depth + 1));
                        return accum;
                    }))
            .Concat(unkeyed
                .Select(field => field.DepthFirstEnumerate(depth + 1))
                .Aggregate(new List<(Expression, int)>(),
                    (accum, item) =>
                    {
                        accum.AddRange(item);
                        return accum;
                    }));

    public override string ToString() => base.ToString() + $" <keyed: {keyed.Count}, unkeyed: {unkeyed.Count}>";
}