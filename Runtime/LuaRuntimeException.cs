namespace Ectoplasm.Runtime;

/// <summary>
/// Represents exceptions caused by Lua-specific errors.
/// </summary>
public class LuaRuntimeException : Exception
{
    public LuaRuntimeException(string message) : base(message) { }

    public LuaRuntimeException(LuaState state, string message) : this(message)
    {
        // TODO: Use state to generate extra debug info
    }
}