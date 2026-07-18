namespace Ectoplasm.Parsing.Statements;

public class Stat_Goto(string targetLabel, ushort line, ushort col) : Statement(line, col)
{
    public string TargetLabel => targetLabel;
    
    /// <summary>
    /// Set of all local variables visible at this goto's position in its block.
    /// </summary>
    public HashSet<LocalVariable>? VisibleLocals;

    /// <summary>
    /// Specific label statement this goto targets, resolved during scope analysis.
    /// </summary>
    public Stat_Label? ResolvedTarget;
    
    public override string ToString() => base.ToString() + $" <{targetLabel}>";
}