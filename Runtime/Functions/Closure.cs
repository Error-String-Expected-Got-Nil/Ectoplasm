namespace Ectoplasm.Runtime.Functions;

/// <summary>
/// An instance object intended to be attached to <see cref="LuaFunction"/> delegates.
/// </summary>
public class Closure(Upvalue[] upvalues, CompiledPrototype[] prototypes)
{
    public readonly Upvalue[] Upvalues = upvalues;
    public readonly CompiledPrototype[] Prototypes = prototypes;
}