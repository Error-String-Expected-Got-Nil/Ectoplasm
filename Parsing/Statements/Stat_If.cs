using System.Text;
using Ectoplasm.Parsing.Expressions;
using Ectoplasm.Utils;

namespace Ectoplasm.Parsing.Statements;

/// <param name="clauses">
/// Ordered list of clauses to evaluate. There is always at least one, which will be the initial 'if' clause. All
/// subsequent clauses will be 'elseif' clauses.
/// </param>
/// <param name="elseBlock">
/// Statements to execute if no clauses are satisfied; the 'else' clause. If null, the if statement had no else clause.
/// </param>
public class Stat_If(List<(Expression Condition, List<Statement> Block)> clauses, List<Statement>? elseBlock, 
    ushort line, ushort col) : Statement(line, col)
{
    public override IEnumerable<Expression> GetExpressions()
        => clauses.Select(clause => clause.Condition);

    public override IEnumerable<(List<Statement> Block, List<LocalVariable>? BlockLocals)> GetBlocks()
    {
        var result = clauses.Select(clause => clause.Block);
        if (elseBlock is not null) result = result.Append(elseBlock);
        return result.Select(item => (item, (List<LocalVariable>?)null));
    }

    protected override void AddToDebugString(StringBuilder str, int depth)
    {
        base.AddToDebugString(str, depth);
        for (var i = 0; i < clauses.Count; i++)
        {
            var keyword = i == 0 ? "if" : "elseif";
            str.AppendRep(".   ", depth + 1, keyword);
            str.AppendRep(".   ", depth + 2, "Condition:");
            clauses[i].Condition.AddToDebugString(str, depth + 3);
            str.AppendRep(".   ", depth + 2, "Block:");
            AddBlockDebugString(str, clauses[i].Block, depth + 3);
        }

        if (elseBlock is null) return;

        str.AppendRep(".   ", depth + 1, "else");
        str.AppendRep(".   ", depth + 2, "Block:");
        AddBlockDebugString(str, elseBlock, depth + 3);
    }
}