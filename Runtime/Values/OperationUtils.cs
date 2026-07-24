using Ectoplasm.Runtime.Functions;
using Ectoplasm.Runtime.Tables;
using System.Runtime.CompilerServices;

namespace Ectoplasm.Runtime.Values;

/// <summary>
/// Nonspecific utilities used in runtime operator implementations.
/// </summary>
public static class OperationUtils
{
    /// <summary>
    /// Indexes a given object's metatable and returns the result. If a metatable could not be resolved for the object,
    /// returns nil.
    /// </summary>
    public static LuaValue GetMetavalue(LuaState state, LuaValue obj, LuaValue index)
    {
        if (obj._kind is LuaValueKind.Table)
            return ((LuaTable)obj._ref).Metatable is { } mt 
                ? mt[index] 
                : default;

        if (obj._kind is LuaValueKind.Userdata)
            return ((LuaUserdata)obj._ref).Metatable is { } mt
                ? mt[index]
                // TODO: Check Lua state for userdata metatable resolver first.
                : throw new NotImplementedException();

        // Object was not a table or userdata, but its type might still have a global metatable set.
        return state.TypeMetatables.TryGetValue(obj._kind, out var tmt)
            ? tmt[index]
            : default;
    }

    /// <summary>
    /// <para>
    /// Determines what <see cref="LuaFunction"/> a given value represents, including consideration for __call 
    /// metamethods. Throws an exception if the value cannot be interpreted as a function.
    /// </para>
    /// <para>
    /// Modifies <see cref="LuaState.StackTop"/>. If the value was a function, sets it to 0. If the function had to be
    /// resolved from a metatable, sets it to 1 and pushes the value, as required for __call metamethods.
    /// </para>
    /// </summary>
    public static LuaFunction ResolveCallable(LuaState state, LuaValue value)
    {
        var depth = 0;
        while (depth < 15)
        {
            if (value._kind is LuaValueKind.Function)
            {
                state.StackTop = depth == 0 ? 0u : 1u;
                if (depth != 0) state.Push(value);
                return (LuaFunction)value._ref;
            }

            value = GetMetavalue(state, value, "__call");
            if (value._kind is LuaValueKind.Nil)
                throw new LuaRuntimeException(state, "Attempt to call a value that was not a function and did not " +
                    "have a valid __call metamethod");

            depth++;
        }

        throw new LuaRuntimeException(state, "__call metamethod chain may not be longer than 15 objects");
    }

    /// <summary>
    /// Attempts to index a metavalue from the given string key in either operand a or b, then executes it using a and
    /// b, returning the result. Throws an exception if it is unable to resolve a function. 
    /// </summary>
    public static LuaValue CallBinaryMetamethod(LuaState state, LuaValue a, LuaValue b, string methodName)
    {
        var method = GetMetavalue(state, a, methodName);
        if (method._kind is LuaValueKind.Nil) method = GetMetavalue(state, b, methodName);
        if (method._kind is LuaValueKind.Nil)
            throw new LuaRuntimeException(state, $"Operation between types {a._kind} and {b._kind} was invalid, and " +
                $"neither had a valid {methodName} metamethod to use instead");

        state.PushStackTop();
        var func = ResolveCallable(state, method);
        state.Push(a);
        state.Push(b);
        state.StackTop += 2;
        func(state);
        state.Adjust(1);
        state.PopStackTop();
        return state.Pop(); 
    }
}
