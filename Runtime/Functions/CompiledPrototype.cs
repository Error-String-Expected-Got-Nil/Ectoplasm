using System.Reflection;
using Ectoplasm.Parsing;

namespace Ectoplasm.Runtime.Functions;

/// <summary>
/// Represents a compiled Lua function prototype. This holds metadata necessary to form closures at runtime, where
/// <see cref="Prototype"/> holds compile-time metadata.
/// </summary>
public class CompiledPrototype
{
    /// <summary>
    /// The actual compiled method for this function.
    /// </summary>
    public readonly MethodInfo Function;

    /// <summary>
    /// An array of child prototypes. The first element will always be this prototype, all remaining elements are the
    /// child prototypes defined within this prototype. Needed so they can be instantiated at runtime. The
    /// self-reference is included for debug information.
    /// </summary>
    public readonly CompiledPrototype[] Prototypes;

    /// <summary>
    /// The original metadata of this prototype generated during compilation. This is usually discarded, but may
    /// optionally be included as extra debug information.
    /// </summary>
    public readonly Prototype? DebugMetadata;

    public CompiledPrototype(MethodInfo function, CompiledPrototype[] children, Prototype? debugMetadata = null)
    {
        Function = function;
        Prototypes = [this, ..children];
        DebugMetadata = debugMetadata;
    }
    
    /// <summary>
    /// Takes an array of upvalues and uses them to produce a new <see cref="LuaFunction"/> instance based on this
    /// prototype.
    /// </summary>
    public LuaFunction Close(Upvalue[] upvalues)
        => Function.CreateDelegate<LuaFunction>(new Closure(upvalues, Prototypes));
}