using System.Text;
using Ectoplasm.Lexing;
using Ectoplasm.Parsing.Expressions;
using Ectoplasm.Utils;

namespace Ectoplasm.Parsing.Statements;

/// <param name="name">Token containing the name of the loop control variable.</param>
/// <param name="initial">Expression producing the initial value of the starting variable.</param>
/// <param name="end">Expression producing the value to end the loop on.</param>
/// <param name="increment">Optional expression to increment the control variable with.</param>
public class Stat_ForNumeric(string name, Expression initial, Expression end, Expression? increment, 
    List<Statement> block, ushort line, ushort col) : Statement(line, col)
{
    protected override void AddToDebugString(StringBuilder str, int depth)
    {
        base.AddToDebugString(str, depth);
        str.AppendRep(".   ", depth + 1, $"Control Variable Name: {name}");
        str.AppendRep(".   ", depth + 1, "Initial:");
        initial.AddToDebugString(str, depth + 2);
        str.AppendRep(".   ", depth + 1, "End:");
        end.AddToDebugString(str, depth + 2);

        if (increment is not null)
        {
            str.AppendRep(".   ", depth + 1, "Increment:");
            increment.AddToDebugString(str, depth + 2);
        }

        str.AppendRep(".   ", depth + 1, "Block:");
        AddBlockDebugString(str, block, depth + 2);
    }
}