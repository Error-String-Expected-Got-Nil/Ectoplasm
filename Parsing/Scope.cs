using Ectoplasm.Parsing.Statements;

namespace Ectoplasm.Parsing;

/// <summary>
/// Represents metadata about a particular lexical scope in a Lua abstract syntax tree. These objects are ephemeral, and
/// should only exist temporarily during scope analysis of a chunk.
/// </summary>
public class Scope(Prototype enclosing, bool isRoot)
{
    /// <summary>
    /// The function prototype this scope belongs to. This is the first prototype encountered when walking up the
    /// abstract syntax tree starting from the location of this scope. Any local variables declared in this scope will
    /// be bound to this prototype.
    /// </summary>
    public readonly Prototype EnclosingPrototype = enclosing;

    /// <summary>
    /// If true, indicates that this is the root scope of the <see cref="EnclosingPrototype"/>.
    /// </summary>
    public readonly bool IsPrototypeRoot = isRoot;

    /// <summary>
    /// All local variable names that have been declared within this scope so far. This is updated as statements within
    /// this scope are analyzed, so it is not considered immutable. 
    /// </summary>
    public readonly Dictionary<string, LocalVariable> DeclaredNames = new();

    /// <summary>
    /// All labels declared within this scope. Should be initialized before proper analysis begins.
    /// </summary>
    public readonly Dictionary<string, Stat_Label> DeclaredLabels = new();

    /// <summary>
    /// All labels declared within this scope which are "terminal". That is, they only appear after the last non-void
    /// statement in the block this scope represents, so it's always safe to jump to them.
    /// </summary>
    public readonly HashSet<Stat_Label> TerminalLabels = [];
    
    /// <summary>
    /// Tracks what local variables are visible at each label declared within this scope. 
    /// </summary>
    public readonly Dictionary<Stat_Label, HashSet<LocalVariable>> LabelVisibleLocals = new();

    /// <summary>
    /// Tracks what local variables are visible at each goto statement within this scope.
    /// </summary>
    public readonly Dictionary<Stat_Goto, HashSet<LocalVariable>> GotoVisibleLocals = new();

    /// <summary>
    /// Initialize a new prototype root scope from a given prototype.
    /// </summary>
    public Scope(Prototype root) : this(root, true)
    {
        foreach (var local in root.Externals) DeclaredNames[local.Name] = local;
        foreach (var local in root.Locals) DeclaredNames[local.Name] = local;
        
        CheckLabels(root.Contents);
    }

    public void CheckLabels(List<Statement> block)
    {
        foreach (var stat in block)
        {
            if (!stat.IsVoid) TerminalLabels.Clear();
            if (stat is not Stat_Label label) continue;
            DeclaredLabels[label.LabelName] = label;
            TerminalLabels.Add(label);
        }
    }
}