namespace Ectoplasm.Parsing.Statements;

public class Stat_Label(string labelName, ushort line, ushort col) : Statement(line, col)
{
    public override bool IsVoid => true;
    
    public string LabelName => labelName;

    /// <summary>
    /// Set of all local variables visible at this label's position in its block.
    /// </summary>
    public HashSet<LocalVariable>? VisibleLocals;
    
    /// <summary>
    /// If true, this label appears after the last non-void statement in its block, and it is always safe to jump to it.
    /// </summary>
    public bool IsTerminal;
    
    public override string ToString() => base.ToString() + $" <{labelName}>";
}