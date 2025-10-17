using Ectoplasm.Runtime.Values;

namespace Ectoplasm.Runtime.Functions;

/// <summary>
/// Basic reference-type container for a <see cref="LuaValue"/>. As implied by the name, this exists primarily for the
/// implementation of function closure upvalues.
/// </summary>
public class Upvalue
{
    public LuaValue Value;

    public Upvalue() { }

    public Upvalue(LuaValue value)
    {
        Value = value;
    }
}