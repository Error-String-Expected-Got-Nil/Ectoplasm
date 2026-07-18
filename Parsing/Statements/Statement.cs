using System.Text;
using Ectoplasm.Parsing.Expressions;
using Ectoplasm.Utils;

namespace Ectoplasm.Parsing.Statements;

public abstract class Statement(ushort line, ushort col)
{
    public ushort StartLine => line;
    public ushort StartCol => col;

    /// <summary>
    /// Indicates if this is a void statement (empty or a label).
    /// </summary>
    public virtual bool IsVoid => false;

    /// <summary>
    /// Indicates if this statement's defines a single sub-block which can be exited by a break statement. 
    /// </summary>
    public virtual bool IsBreakable => false;

    /// <summary>
    /// Get all expressions contained within this statement. Returns null if this expression doesn't contain any
    /// expressions.
    /// </summary>
    public virtual IEnumerable<Expression>? GetExpressions() => null;

    /// <summary>
    /// Adds any local variables declared by this statement to the list of locals for the given prototype.
    /// </summary>
    /// <returns>
    /// List containing any local variables declared which are visible in the same scope this statement executes in. If
    /// null, no such locals were declared.
    /// </returns>
    public virtual List<LocalVariable>? DeclareLocals(Prototype prototype) => null;

    /// <summary>
    /// Get an enumeration of all sub-blocks of this statement, or null if this statement has no sub-blocks. Each block
    /// is optionally bundled with list containing any visible local variables that are implicitly declared as part of
    /// this block, though this is currently only used by for loops. These local variables will have been declared in
    /// <see cref="DeclareLocals"/>, but not yet returned.
    /// </summary>
    public virtual IEnumerable<(List<Statement> Block, List<LocalVariable>? BlockLocals)>? GetBlocks() => null;
    
    public override string ToString() => $"{GetType().Name} [{StartLine}, {StartCol}]";
    
    protected virtual void AddToDebugString(StringBuilder str, int depth) 
        => str.AppendRep(".   ", depth).Append(ToString()).AppendLine();
    
    public static string GetBlockDebugString(List<Statement> block, int baseDepth = 0)
        => AddBlockDebugString(new StringBuilder(), block, baseDepth).ToString();

    public static StringBuilder AddBlockDebugString(StringBuilder str, List<Statement> block, 
        int baseDepth = 0)
    {
        foreach (var stat in block) stat.AddToDebugString(str, baseDepth);
        return str;
    }
}