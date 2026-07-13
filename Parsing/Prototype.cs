using Ectoplasm.Parsing.Expressions;
using Ectoplasm.Parsing.Statements;
using Ectoplasm.Runtime.Functions;

namespace Ectoplasm.Parsing;

/// <summary>
/// Represents a Lua function prototype.
/// </summary>
public class Prototype
{
    /// <summary>
    /// Dummy prototype object used as a placeholder when a local variable must be considered external without having
    /// it actually be bound to any real prototype. This is in particular used as the origin for the _ENV upvalue which
    /// holds the global environment that the main chunk references (see reference manual version 5.4, section 2.2).
    /// </summary>
    internal static readonly Prototype ExternalDummy = new();
    
    /// <summary>
    /// The parent prototype whose body this prototype is defined in. If this is null, this is a main chunk prototype,
    /// and therefore has no parent.
    /// </summary>
    public readonly Prototype? Parent;

    /// <summary>
    /// Parameter names for this prototype.
    /// </summary>
    public readonly List<string> Parameters = [];

    /// <summary>
    /// If true, this is a variable-argument function. Once compiled, an additional local variable slot will be
    /// allocated beyond the count of <see cref="Locals"/>, which will be used to contain an array of the extra
    /// arguments provided when the function is called.
    /// </summary>
    public readonly bool IsVararg;

    /// <summary>
    /// The line of the source file this prototype originates from where the prototype was originally defined, or 0 if
    /// unspecified or not applicable.
    /// </summary>
    public readonly ushort Line;

    /// <summary>
    /// The column of the line of the source file this prototype originates from where the prototype was originally
    /// defined, or 0 if unspecified or not applicable.
    /// </summary>
    public readonly ushort Col;
    
    /// <summary>
    /// Debug name used for this function. 
    /// </summary>
    public readonly string? Name;

    /// <summary>
    /// Name of the source file this prototype originates from.
    /// </summary>
    public readonly string? SourceName;
    
    /// <summary>
    /// List of all local variables which originate in this prototype. If the prototype has named parameters, those will
    /// be the first locals in this list.
    /// </summary>
    public readonly List<LocalVariable> Locals = [];

    /// <summary>
    /// List of all variables referenced in this prototype but which do not originate in it, hence "external". All of
    /// these will have <see cref="LocalVariable.ExternalSource"/> as non-null, and it will point to a local variable in
    /// the parent prototype.
    /// </summary>
    public readonly List<LocalVariable> Externals = [];

    /// <summary>
    /// The raw code block of this prototype.
    /// </summary>
    public readonly List<Statement> Contents = [];

    /// <summary>
    /// List of all child prototypes defined within this prototype. Note that, in a <see cref="CompiledPrototype"/>, the
    /// first element of its <see cref="CompiledPrototype.Prototypes"/> array will be a self-reference, so these are
    /// effectively indexed 1-based once compiled.
    /// </summary>
    public readonly List<Prototype> Children = [];

    private Prototype() { }

    public Prototype(Prototype parent, Expr_FunctionDef def, string? sourceName = null)
        : this(parent, def.Parameters, def.IsVararg, def.Body, def.StartLine, def.StartCol, def.DebugName, sourceName)
    { }
    
    public Prototype(Prototype? parent, List<string> parameters, bool isVararg, List<Statement> contents, 
        ushort line = 0, ushort col = 0, string? name = null, string? sourceName = null)
    {
        Parent = parent;
        Parameters = parameters;
        IsVararg = isVararg;
        Contents = contents;
        Line = line;
        Col = col;
        Name = name;
        SourceName = sourceName ?? parent?.SourceName;

        var env = new LocalVariable(this, "_ENV")
        {
            IsUpvalue = true,
            ExternalSource = parent is null 
                // No parent, this is a main chunk, so we need to create a dummy variable that represents the external
                // global environment.
                ? new LocalVariable(ExternalDummy, "_ENV") { IsUpvalue = true } 
                // All well-formed prototypes will have their first external variable (upvalue) as the global
                // environment table, so this should always be fine.
                : parent.Externals[0]
        };
        
        Externals.Add(env);

        foreach (var param in parameters) AddNewLocal(param);
    }

    /// <summary>
    /// Creates a new local variable, adds it to this prototype's locals, sets its index, then returns it.
    /// </summary>
    public LocalVariable AddNewLocal(string name, LocalAttribute attr = LocalAttribute.None)
    {
        var local = new LocalVariable(this, name, attr) { Index = Locals.Count };
        Locals.Add(local);
        return local;
    }

    /// <summary>
    /// Creates a new local variable that references a given external variable.
    /// </summary>
    public LocalVariable AddNewExternal(LocalVariable source)
    {
        if (source.Owner != Parent)
            throw new ArgumentException("Attempt to add external local variable to function prototype that is not " +
                "owned by the prototype's parent");
        if (!source.IsUpvalue)
            throw new ArgumentException("Attempt to add external local variable with non-upvalue source");
        
        var external = new LocalVariable(this, source.Name, source.Attribute)
        {
            IsUpvalue = true,
            ExternalSource = source,
            Index = Externals.Count
        };
        Externals.Add(external);
        return external;
    }
}