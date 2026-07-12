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
}