using System.Text;
using Ectoplasm.Parsing.Expressions;
using Ectoplasm.Utils;

namespace Ectoplasm.Parsing.Statements;

public class Stat_While(Expression condition, List<Statement> contents, ushort line, ushort col) : Statement(line, col)
{
    public override bool IsBreakable => true;
    
    public override IEnumerable<Expression> GetExpressions() => [condition];

    public override IEnumerable<(List<Statement> Block, List<LocalVariable>? BlockLocals)>? GetBlocks() 
        => [(contents, null)];

    protected override void AddToDebugString(StringBuilder str, int depth)
    {
        base.AddToDebugString(str, depth);
        str.AppendRep(".   ", depth + 1, "Condition:");
        condition.AddToDebugString(str, depth + 2);
        str.AppendRep(".   ", depth + 1, "Contents:");
        AddBlockDebugString(str, contents, depth + 2);
    }
}