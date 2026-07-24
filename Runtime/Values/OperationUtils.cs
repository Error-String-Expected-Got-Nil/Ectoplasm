using Ectoplasm.Runtime.Functions;
using Ectoplasm.Runtime.Tables;
using static Ectoplasm.Runtime.LuaValueKind;

namespace Ectoplasm.Runtime.Values;

/// <summary>
/// Nonspecific utilities used in runtime operator implementations.
/// </summary>
public static class OperationUtils
{
    /// <summary>
    /// Takes two Lua values and attempts to match their types for standard arithmetic operations like addition. Returns
    /// the type both were coerced to, or <see cref="Nil"/> if a coercion could not be made (and metatables must be
    /// checked).
    /// </summary>
    public static LuaValueKind MatchOperandTypes(ref LuaValue a, ref LuaValue b)
    {
        // Operands are not both numbers, cannot coerce them.
        if (!(a._kind is Integer or Float && b._kind is Integer or Float)) return Nil;

        // Types match, no coercion necessary.
        if (a._kind is Integer && b._kind is Integer) return Integer;
        if (a._kind is Float && b._kind is Float) return Float;

        // Operand types are different but are either integer or float.
        // First is integer, so second is a float, have to cast first.
        if (a._kind is Integer)
        {
            a = new LuaValue { _kind = Float, _float = a._integer };
            return Float;
        }

        // First must be a float and the second an integer, cast second.
        b = new LuaValue { _kind = Float, _float = b._integer };
        return Float;
    }

    /// <summary>
    /// Similar to <see cref="MatchOperandTypes"/>, except it tries to coerce towards integers wherever possible, and
    /// returns <see cref="Nil"/> if it can't for both. This is for bitwise operations.
    /// </summary>
    public static LuaValueKind MatchOperandTypesInt(ref LuaValue a, ref LuaValue b)
    {
        if (!(a._kind is Integer or Float && b._kind is Integer or Float)) return Nil;

        if (a._kind is Integer && b._kind is Integer) return Integer;
        if (!a.TryCoerceInteger(out var aInt) || !b.TryCoerceInteger(out var bInt)) return Nil;

        a = new LuaValue { _kind = Integer, _integer = aInt };
        b = new LuaValue { _kind = Integer, _integer = bInt };
        return Integer;
    }

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
            if (value._kind is Function)
            {
                state.StackTop = depth == 0 ? 0u : 1u;
                if (depth != 0) state.Push(value);
                return (LuaFunction)value._ref;
            }

            value = GetMetavalue(state, value, "__call");
            if (value._kind is Nil)
                throw new LuaRuntimeException(state, "Attempt to call a value that was not a function and did not " +
                    "have a valid __call metamethod");

            depth++;
        }

        throw new LuaRuntimeException(state, "__call metamethod chain may not be longer than 15 objects");
    }

    /// <summary>
    /// Attempts to index a metavalue from the given string key in either operand a or b, then executes it using a and
    /// b, returning the result adjusted to one value. Throws an exception if it is unable to resolve a function. 
    /// </summary>
    public static LuaValue CallBinaryMetamethod(LuaState state, LuaValue a, LuaValue b, string methodName)
    {
        var method = GetMetavalue(state, a, methodName);
        if (method._kind is Nil) method = GetMetavalue(state, b, methodName);
        if (method._kind is Nil)
            throw new LuaRuntimeException(state, $"Operation between values of type {a._kind} and {b._kind} was " +
                $"invalid, and neither had a valid {methodName} metamethod to use instead");

        var prevTop = state.StackTop;
        var func = ResolveCallable(state, method);
        state.Push(a);
        state.Push(b);
        state.StackTop += 2;
        func(state);
        state.Adjust(1);
        state.StackTop = prevTop;
        return state.Pop(); 
    }

    /// <summary>
    /// Like the binary version of this, except it applies to unary metamethods instead, with one operand.
    /// </summary>
    public static LuaValue CallUnaryMetamethod(LuaState state, LuaValue value, string methodName)
    {
        var method = GetMetavalue(state, value, methodName);
        if (method._kind is LuaValueKind.Nil)
            throw new LuaRuntimeException(state, $"Unary operation on value of type {value._kind} was invalid, and " +
                $"it did not have a valid {methodName} metamethod to use instead");

        var prevTop = state.StackTop;
        var func = ResolveCallable(state, method);
        state.Push(value);
        state.StackTop++;
        func(state);
        state.Adjust(1);
        state.StackTop = prevTop;
        return state.Pop();
    }

    /// <summary>
    /// Performs a bitshift according to Lua's rules for the operation (see the Lua reference manual version 5.4, 
    /// section 3.4.2). Positive shift is to the right, negative is to the left.
    /// </summary>
    public static long LuaBitshift(long value, long shift)
    {
        if (shift == 0) return value;
        if (shift > 63 || shift < -63) return 0;
        if (shift > 0) return value >>> (int)shift;
        return value << (int)-shift;
    }
}
