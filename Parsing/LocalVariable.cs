using Ectoplasm.Runtime.Functions;
using Ectoplasm.Runtime.Values;

namespace Ectoplasm.Parsing;

/// <summary>
/// Represents parsing metadata for a single local variable unique to a particular Lua function prototype.
/// </summary>
public class LocalVariable(Prototype owner, string name, LocalAttribute attr = LocalAttribute.None)
{
    /// <summary>
    /// The function prototype which owns this local variable.
    /// </summary>
    public readonly Prototype Owner = owner;

    /// <summary>
    /// A name for this local variable that can be used in debug information associated with it. If this was an
    /// explicitly-declared local, it will be the name used to declare it. If this was an implicitly-declared local
    /// (such as is created for storing for loop increment and limit values), it will be arbitrary, but will always
    /// include characters that are not normally legal in a name, in order to distinguish it.
    /// </summary>
    public readonly string Name = name;

    /// <summary>
    /// The attribute of this local variable, if any. If <see cref="ExternalSource"/> is non-null, this should always be
    /// the same as the attribute of the <see cref="ExternalSource"/>.
    /// </summary>
    public readonly LocalAttribute Attribute = attr;
    
    /// <summary>
    /// <para>
    /// If true, this local variable should be stored wrapped in an <see cref="Upvalue"/> when compiled. 
    /// </para>
    /// <para>
    /// This is set when either this variable is external (in which case <see cref="ExternalSource"/> should also be
    /// non-null), or when a child prototype references this variable.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// When a function prototype references a local variable in an external prototype scope (parent or other ancestor),
    /// that variable must be boxed so that it can persist after its owning prototype scope exits, so that the closure
    /// referencing it may continue to use it.
    /// </para>
    /// <para>
    /// In practice, the easiest way to handle this is to simply wrap the <see cref="LuaValue"/> in an
    /// <see cref="Upvalue"/> on creation, and then access the contents of that <see cref="Upvalue"/> any time the
    /// variable is referenced. This indicates that this variable should be treated as such, if true.
    /// </para>
    /// </remarks>
    public bool IsUpvalue;

    /// <summary>
    /// <para>
    /// If non-null and <see cref="IsUpvalue"/> is true, this indicates the external local variable in the parent
    /// prototype scope which this variable references.
    /// </para>
    /// <para>
    /// Note that this will only ever be a local variable in a *parent* scope. If the true variable referenced
    /// originates in some greater ancestor than the parent, the local variable this field references will itself have
    /// an external source, and so on until reaching the original.
    /// </para>
    /// </summary>
    public LocalVariable? ExternalSource;

    /// <summary>
    /// The index used to access this variable in the context of its owning prototype. If <see cref="ExternalSource"/>
    /// is null, this is an index into the method's compiled local variable list. If <see cref="ExternalSource"/> is
    /// non-null, this is an index into the prototype's upvalue array.
    /// </summary>
    public int Index;

    /// <summary>
    /// If true, this local variable was implicitly declared (for example, a for loop's control variable).
    /// </summary>
    public bool IsImplicit = false;
}