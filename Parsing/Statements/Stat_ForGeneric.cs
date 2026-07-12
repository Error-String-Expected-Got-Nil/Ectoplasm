using System.Text;
using Ectoplasm.Lexing;
using Ectoplasm.Parsing.Expressions;
using Ectoplasm.Utils;

namespace Ectoplasm.Parsing.Statements;

/// <param name="namelist">List of loop variables. First is control variable.</param>
/// <param name="explist">Expressions that define loop parameters.</param>
public class Stat_ForGeneric(List<string> namelist, List<Expression> explist, List<Statement> block, ushort line, 
    ushort col) : Statement(line, col)
{
    protected override void AddToDebugString(StringBuilder str, int depth)
    {
        base.AddToDebugString(str, depth);
        str.AppendRep(".   ", depth + 1, "Namelist:");
        foreach (var name in namelist) str.AppendRep(".   ", depth + 2, name);

        str.AppendRep(".   ", depth + 1, "Expressions:");
        for (var i = 0; i < explist.Count; i++)
        {
            var expr = explist[i];
            str.AppendRep(".   ", depth + 2, $"Expression {i}:");
            expr.AddToDebugString(str, depth + 3);
        }

        str.AppendRep(".   ", depth + 1, "Block:");
        AddBlockDebugString(str, block, depth + 2);
    }
}