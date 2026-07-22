using Ectoplasm.Runtime.Tables;
using Ectoplasm.Runtime.Values;
using Ectoplasm.Utils;

namespace Ectoplasm.Runtime;

/// <summary>
/// Holds global runtime state about a particular Lua instance, such as the Lua stack and the global environment.
/// </summary>
public class LuaState
{
    private readonly TransparentStack<LuaValue> _stack = new();

    /// <summary>
    /// Temporary storage for StackTop values. Used primarily for function calls which have function calls as 
    /// arguments.
    /// </summary>
    private readonly Stack<int> _nestedStackTops = [];

    /// <summary>
    /// For types other than <see cref="LuaValueKind.Table"/> and <see cref="LuaValueKind.Userdata"/>, which have 
    /// individual metatables, each type may or may not have an associated metatable used by all instances of that
    /// type. These are stored (and may be modified) here.
    /// </summary>
    public readonly Dictionary<LuaValueKind, LuaTable> TypeMetatables = [];

    /// <summary>
    /// <para>
    /// The number of function arguments or return values currently on top of the stack.
    /// </para>
    /// <para>
    /// This must be set manually and directly before this LuaState is used to call a function unless otherwise stated.
    /// </para>
    /// </summary>
    public int StackTop;

    public void Push(LuaValue value) => _stack.Push(value);

    public LuaValue Pop() => _stack.Pop();

    /// <summary>
    /// Push the current <see cref="StackTop"/> into an internal stack and reset it to 0.
    /// </summary>
    public void PushStackTop()
    {
        _nestedStackTops.Push(StackTop);
        StackTop = 0;
    }

    /// <summary>
    /// Overwrite the current <see cref="StackTop"/> with the last pushed value.
    /// </summary>
    public void PopStackTop() => StackTop = _nestedStackTops.Pop();
}