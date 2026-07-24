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
    private readonly Stack<uint> _nestedStackTops = [];

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
    public uint StackTop;

    /// <summary>
    /// <para>
    /// Push a value on top of the state's value stack. 
    /// </para>
    /// <para>
    /// Does not interact with <see cref="StackTop"/>, you must modify it manually as necessary after using this.
    /// </para>
    /// </summary>
    public void Push(LuaValue value) => _stack.Push(value);

    /// <summary>
    /// <para>
    /// Pop a value from the top of the state's value stack.
    /// </para>
    /// <para>
    /// Does not interact with <see cref="StackTop"/>, you must modify it manually as necessary after using this.
    /// </para>
    /// </summary>
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

    /// <summary>
    /// Increases or decreases the number of elements on top of the stack (based on <see cref="StackTop"/>) to make it
    /// equal to the given count. When increasing, adds nil values to reach the count. When decreasing, truncates.
    /// This also modifies StackTop to match the new value afterward.
    /// </summary>
    public void Adjust(uint toCount)
    {
        if (toCount == StackTop) return;
        
        if (toCount < StackTop)
        {
            _stack.PopMany((int)(StackTop - toCount));
            StackTop = toCount;
            return;
        }
        
        for (var i = 0; i < toCount - StackTop; i++) _stack.Push(default);
        StackTop = toCount;
    }
}