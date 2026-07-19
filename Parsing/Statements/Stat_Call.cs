using System.Text;
using Ectoplasm.Parsing.Expressions;

namespace Ectoplasm.Parsing.Statements;

public class Stat_Call(Expression callExpr, ushort line, ushort col) : Statement(line, col)
{
    public override IEnumerable<Expression> GetExpressions() => [callExpr];

    protected override void AddToDebugString(StringBuilder str, int depth)
    {
        base.AddToDebugString(str, depth);
        callExpr.AddToDebugString(str, depth + 1);
    }
}