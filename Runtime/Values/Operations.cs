using Ectoplasm.Runtime.Tables;
using System.Diagnostics;
using static Ectoplasm.Runtime.LuaValueKind;

namespace Ectoplasm.Runtime.Values;

/// <summary>
/// Static class holding implementations for non-arithmetic operations, including relational operators, logical 
/// operators, length, and concatenation.
/// </summary>
public static class Operations
{
    public static LuaValue Concat(LuaState state, LuaValue a, LuaValue b)
    {
        if (!(a._kind is LuaValueKind.String or Integer or Float)
            && (b._kind is LuaValueKind.String or Integer or Float))
            return OperationUtils.CallBinaryMetamethod(state, a, b, "__concat");

        var left = GetString(a);
        var right = GetString(b);

        return left + right;

        static string GetString(LuaValue value)
            => value._kind switch
            {
                Integer => value._integer.ToString(),
                Float => value._float.ToString(),
                LuaValueKind.String => (string)value._ref,
                _ => throw new UnreachableException()
            };
    }

    /// <summary>
    /// The length operation, accounting for metamethods. Note that there is a slight deviation from the specification
    /// here: While the reference manual (version 5.4, section 3.4.7) states it should return the number of bytes in a
    /// string, this returns the number of characters, which will be half that due to the use of UTF-16 strings. This
    /// is to preserve the expected behavior.
    /// </summary>
    public static LuaValue Length(LuaState state, LuaValue value)
    {
        if (value._kind is LuaValueKind.String) return ((string)value._ref).Length;
        
        var metamethod = OperationUtils.GetMetavalue(state, value, "__len");
        if (metamethod._kind is Nil)
        {
            if (value._kind is Table) return ((LuaTable)value._ref).Length;
            throw new LuaRuntimeException(state, $"Attempt to get length of value with type {value._kind} that had " +
                $"no __len metamethod");
        }

        var prevTop = state.StackTop;
        var func = OperationUtils.ResolveCallable(state, metamethod);
        state.Push(value);
        state.StackTop++;
        func(state);
        state.Adjust(1);
        state.StackTop = prevTop;
        return state.Pop();
    }

    public static LuaValue EqualTo(LuaState state, LuaValue a, LuaValue b, bool invert)
    {
        OperationUtils.MatchOperandTypes(ref a, ref b);
        if (a._kind != b._kind) return invert;
        return a._kind switch
        {
            Nil => !invert,
            LuaValueKind.Boolean => (a._boolean == b._boolean) ^ invert,
            Integer => (a._integer == b._integer) ^ invert,
            Float => (a._float == b._float) ^ invert,
            LuaValueKind.String => ((string)a._ref == (string)b._ref) ^ invert,
            Table or Userdata => a._ref == b._ref
                ? !invert
                : OperationUtils.CallBinaryMetamethod(state, a, b, "__eq").IsTruthy ^ invert,
            _ => (a._ref == b._ref) ^ invert
        };
    }

    public static LuaValue LessThan(LuaState state, LuaValue a, LuaValue b)
    {
        var type = OperationUtils.MatchOperandTypes(ref a, ref b);

        if (type is Integer) return a._integer < b._integer;
        if (type is Float) return a._float < b._float;
        if (a._kind is LuaValueKind.String && b._kind is LuaValueKind.String)
            return string.Compare((string)a._ref, (string)b._ref) < 0;

        return OperationUtils.CallBinaryMetamethod(state, a, b, "__lt");
    }

    public static LuaValue LessThanOrEqualTo(LuaState state, LuaValue a, LuaValue b)
    {
        var type = OperationUtils.MatchOperandTypes(ref a, ref b);

        if (type is Integer) return a._integer <= b._integer;
        if (type is Float) return a._float <= b._float;
        if (a._kind is LuaValueKind.String && b._kind is LuaValueKind.String)
            return string.Compare((string)a._ref, (string)b._ref) <= 0;

        return OperationUtils.CallBinaryMetamethod(state, a, b, "__le");
    }

    public static LuaValue GetIndex(LuaState state, LuaValue value, LuaValue key)
    {
        if (value._kind is Table)
        {
            var res = ((LuaTable)value._ref)[key];
            if (res._kind is not Nil) return res;
        }

        var alt = OperationUtils.GetMetavalue(state, value, "__index");
        switch (alt._kind)
        {
            case Nil:
                if (value._kind is not Table)
                    throw new LuaRuntimeException(state, $"Attempt to index a value of type {value._kind} that did not " +
                        $"have an __index metamethod");
                return default;
            case Function:
                var prevTop = state.StackTop;
                var func = OperationUtils.ResolveCallable(state, alt);
                state.Push(value);
                state.Push(key);
                state.StackTop += 2;
                func(state);
                state.Adjust(1);
                state.StackTop = prevTop;
                return state.Pop();
            default:
                return GetIndex(state, alt, key);

        }
    }

    public static void SetIndex(LuaState state, LuaValue table, LuaValue key, LuaValue value)
    {
        if (table._kind is Table)
        {
            var castTable = (LuaTable)table._ref;
            // A table is considered not to contain a value for a key only when indexing the key returns nil.
            if (castTable[key]._kind != Nil)
            {
                castTable[key] = value;
                return;
            }
        }

        var alt = OperationUtils.GetMetavalue(state, table, "__newindex");
        switch (alt._kind)
        {
            case Nil:
                if (table._kind is not Table) 
                    throw new LuaRuntimeException(state, $"Attempt to assign to a value of type {table._kind} that " +
                        $"did not have a __newindex metamethod");
                ((LuaTable)table._ref)[key] = value;
                return;
            case Function:
                // Saving StackTop might not be necessary since this can't be part of an expression, but it can't hurt.
                var prevTop = state.StackTop;
                var func = OperationUtils.ResolveCallable(state, alt);
                state.Push(table);
                state.Push(key);
                state.Push(value);
                state.StackTop += 3;
                func(state);
                state.Adjust(0);
                state.StackTop = prevTop;
                return;
            default:
                SetIndex(state, alt, key, value);
                return;
        }
    }
}
