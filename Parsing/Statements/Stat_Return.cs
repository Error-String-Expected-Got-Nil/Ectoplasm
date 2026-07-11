using System.Text;
using Ectoplasm.Parsing.Expressions;
using Ectoplasm.Utils;

namespace Ectoplasm.Parsing.Statements;

public class Stat_Return(List<Expression> returnValues, ushort line, ushort col) : Statement(line, col)
{
    protected override void AddToDebugString(StringBuilder str, int depth)
    {
        base.AddToDebugString(str, depth);
        for (var i = 0; i < returnValues.Count; i++)
        {
            var ret = returnValues[i];
            str.AppendRep(".   ", depth + 1, $"Return {i}:");
            ret.AddToDebugString(str, depth + 2);
        }
    }
}