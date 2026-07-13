namespace Ectoplasm.Parsing.Statements;

public class Stat_Goto(string targetLabel, ushort line, ushort col) : Statement(line, col)
{
    /// <summary>
    /// Set of all local variables visible at this goto's position in its block.
    /// </summary>
    public readonly HashSet<LocalVariable> VisibleLocals = [];
    
    public override string ToString() => base.ToString() + $" <{targetLabel}>";
}