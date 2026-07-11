using System.Text;

namespace Ectoplasm.Parsing.Statements;

public class Stat_Do(List<Statement> contents, ushort line, ushort col) : Statement(line, col)
{
    protected override void AddToDebugString(StringBuilder str, int depth)
    {
        base.AddToDebugString(str, depth);
        GetBlockDebugStringInternal(str, contents, depth + 1);
    }
}