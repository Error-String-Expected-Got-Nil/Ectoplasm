using System.Text;
using Ectoplasm.Utils;

namespace Ectoplasm.Parsing.Statements;

public abstract class Statement(ushort line, ushort col)
{
    public ushort StartLine => line;
    public ushort StartCol => col;

    public override string ToString() => $"{GetType().Name} [{StartLine}, {StartCol}]";
    
    protected virtual void AddToDebugString(StringBuilder str, int depth) 
        => str.AppendRep(".   ", depth).Append(ToString()).AppendLine();
    
    public static string GetBlockDebugString(List<Statement> block, int baseDepth = 0)
        => GetBlockDebugStringInternal(new StringBuilder(), block, baseDepth).ToString();

    protected static StringBuilder GetBlockDebugStringInternal(StringBuilder str, List<Statement> block, 
        int baseDepth = 0)
    {
        foreach (var stat in block) stat.AddToDebugString(str, baseDepth);
        return str;
    }
}