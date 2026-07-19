using System.Text;

namespace Ectoplasm.Parsing.Statements;

public class Stat_Do(List<Statement> contents, ushort line, ushort col) : Statement(line, col)
{
    public override IEnumerable<(List<Statement> Block, List<LocalVariable>? BlockLocals)> GetBlocks()
        => [(contents, null)];

    protected override void AddToDebugString(StringBuilder str, int depth)
    {
        base.AddToDebugString(str, depth);
        AddBlockDebugString(str, contents, depth + 1);
    }
}