using System.Reflection;
using Ectoplasm.Parsing;

namespace Ectoplasm.Runtime.Functions;

/// <summary>
/// Represents a compiled Lua function prototype. This holds metadata necessary to form closures at runtime, where
/// <see cref="Prototype"/> holds compile-time metadata.
/// </summary>
public class CompiledPrototype(MethodInfo function, CompiledPrototype[] children, Prototype? debugMetadata = null)
{
    /// <summary>
    /// The actual compiled method for this function.
    /// </summary>
    public readonly MethodInfo Function = function;
    
    /// <summary>
    /// A list of all child prototypes of this prototype, which is required in order to instantiate them at runtime.
    /// </summary>
    public readonly CompiledPrototype[] Children = children;

    /// <summary>
    /// The original metadata of this prototype generated during compilation. This is usually discarded, but may
    /// optionally be included as extra debug information.
    /// </summary>
    public readonly Prototype? DebugMetadata = debugMetadata;

    /// <summary>
    /// Takes an array of upvalues and uses them to produce a new <see cref="LuaFunction"/> instance based on this
    /// prototype.
    /// </summary>
    public LuaFunction Close(Upvalue[] upvalues)
        => Function.CreateDelegate<LuaFunction>(new Closure(upvalues, Children));
}