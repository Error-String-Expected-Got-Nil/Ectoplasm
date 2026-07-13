using Ectoplasm.Parsing.Statements;

namespace Ectoplasm.Parsing;

/// <summary>
/// Represents metadata about a particular lexical scope in a Lua abstract syntax tree. These objects are ephemeral, and
/// should only exist temporarily during scope analysis of a chunk.
/// </summary>
public class Scope
{
    /// <summary>
    /// The function prototype this scope belongs to. This is the first prototype encountered when walking up the
    /// abstract syntax tree starting from the location of this scope. Any local variables declared in this scope will
    /// be bound to this prototype.
    /// </summary>
    public readonly Prototype EnclosingPrototype;

    /// <summary>
    /// If true, indicates that this is the root scope of the <see cref="EnclosingPrototype"/>.
    /// </summary>
    public readonly bool IsPrototypeRoot;

    /// <summary>
    /// Contents of the block this scope represents.
    /// </summary>
    public readonly List<Statement> Contents;

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
    
    public Scope(Prototype enclosing, List<Statement> contents)
    {
        EnclosingPrototype = enclosing;
        IsPrototypeRoot = false;
        Contents = contents;
        
        CheckLabels(Contents, enclosing.SourceName);
    }
    
    /// <summary>
    /// Initialize a new prototype root scope from a given prototype.
    /// </summary>
    public Scope(Prototype root) : this(root, root.Contents)
    {
        IsPrototypeRoot = true;
        
        // Note that this assumes all external and local variables in the prototype are visible, but this will always
        // be the case under normal circumstances for a root prototype.
        foreach (var local in root.Externals) DeclaredNames[local.Name] = local;
        foreach (var local in root.Locals) DeclaredNames[local.Name] = local;
    }

    private void CheckLabels(List<Statement> block, string? sourceName = null)
    {
        foreach (var stat in block)
        {
            if (!stat.IsVoid) TerminalLabels.Clear();
            if (stat is not Stat_Label label) continue;

            if (DeclaredLabels.TryGetValue(label.LabelName, out var conflicting))
                throw new LuaParsingException($"Label has same name as other label in same block at line " +
                    $"{conflicting.StartLine}, column {conflicting.StartCol}", label.StartLine, label.StartCol, 
                    sourceName);

            DeclaredLabels[label.LabelName] = label;
            TerminalLabels.Add(label);
        }

        foreach (var label in TerminalLabels) label.IsTerminal = true;
    }
}