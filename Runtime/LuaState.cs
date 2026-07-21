using Ectoplasm.Runtime.Values;
using Ectoplasm.Utils;

namespace Ectoplasm.Runtime;

/// <summary>
/// Holds global runtime state about a particular Lua instance, such as the Lua stack and the global environment.
/// </summary>
public class LuaState
{
    private readonly TransparentStack<LuaValue> _stack = new();
}