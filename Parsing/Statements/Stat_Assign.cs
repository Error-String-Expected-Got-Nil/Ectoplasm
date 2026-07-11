using System.Text;
using Ectoplasm.Parsing.Expressions;
using Ectoplasm.Utils;

namespace Ectoplasm.Parsing.Statements;

public class Stat_Assign(List<Expression> variables, List<Expression> values, ushort line, ushort col) 
    : Statement(line, col)
{
    protected override void AddToDebugString(StringBuilder str, int depth)
    {
        base.AddToDebugString(str, depth);
        
        str.AppendRep(".   ", depth + 1, "Variables:");
        for (var i = 0; i < variables.Count; i++)
        {
            var expr = variables[i];
            str.AppendRep(".   ", depth + 2, $"Variable {i}:");
            expr.AddToDebugString(str, depth + 3);
        }
        
        str.AppendRep(".   ", depth + 1, "Values:");
        for (var i = 0; i < values.Count; i++)
        {
            var expr = values[i];
            str.AppendRep(".   ", depth + 2, $"Value {i}:");
            expr.AddToDebugString(str, depth + 3);
        }
    }
}