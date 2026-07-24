using System.Diagnostics;
using static Ectoplasm.Runtime.LuaValueKind;
// ReSharper disable SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault

namespace Ectoplasm.Runtime.Values;

/// <summary>
/// Static class holding the implementations for basic arithmetic operations.
/// </summary>
public static class Arithmetic
{
    /// <summary>
    /// Takes two Lua values and attempts to match their types for standard arithmetic operations like addition. Returns
    /// the type both were coerced to, or <see cref="Nil"/> if a coercion could not be made (and metatables must be
    /// checked).
    /// </summary>
    private static LuaValueKind MatchOperandTypes(ref LuaValue a, ref LuaValue b)
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
    private static LuaValueKind MatchOperandTypesInt(ref LuaValue a, ref LuaValue b)
    {
        if (!(a._kind is Integer or Float && b._kind is Integer or Float)) return Nil;
        
        if (a._kind is Integer && b._kind is Integer) return Integer;
        if (!a.TryCoerceInteger(out var aInt) || !b.TryCoerceInteger(out var bInt)) return Nil;

        a = new LuaValue { _kind = Integer, _integer = aInt };
        b = new LuaValue { _kind = Integer, _integer = bInt };
        return Integer;
    }

    public static LuaValue Add(LuaState state, LuaValue a, LuaValue b)
        => MatchOperandTypes(ref a, ref b) switch
        {
            Integer => a._integer + b._integer,
            Float => a._float + b._float,
            Nil => OperationUtils.CallBinaryMetamethod(state, a, b, "__add"),
            _ => throw new UnreachableException()
        };
    
    public static LuaValue Sub(LuaState state, LuaValue a, LuaValue b)
        => MatchOperandTypes(ref a, ref b) switch
        {
            Integer => a._integer - b._integer,
            Float => a._float - b._float,
            Nil => OperationUtils.CallBinaryMetamethod(state, a, b, "__sub"),
            _ => throw new UnreachableException()
        };
    
    public static LuaValue Mul(LuaState state, LuaValue a, LuaValue b)
        => MatchOperandTypes(ref a, ref b) switch
        {
            Integer => a._integer * b._integer,
            Float => a._float * b._float,
            Nil => OperationUtils.CallBinaryMetamethod(state, a, b, "__mul"),
            _ => throw new UnreachableException()
        };
    
    public static LuaValue Div(LuaState state, LuaValue a, LuaValue b)
        => MatchOperandTypes(ref a, ref b) switch
        {
            Integer => (double)a._integer / b._integer,
            Float => a._float / b._float,
            Nil => OperationUtils.CallBinaryMetamethod(state, a, b, "__div"),
            _ => throw new UnreachableException()
        };
    
    public static LuaValue Mod(LuaState state, LuaValue a, LuaValue b)
        => MatchOperandTypes(ref a, ref b) switch
        {
            Integer => a._integer % b._integer,
            Float => a._float % b._float,
            Nil => OperationUtils.CallBinaryMetamethod(state, a, b, "__mod"),
            _ => throw new UnreachableException()
        };
    
    public static LuaValue Pow(LuaState state, LuaValue a, LuaValue b)
        => MatchOperandTypes(ref a, ref b) switch
        {
            Integer => Math.Pow(a._integer, b._integer),
            Float => Math.Pow(a._float, b._float),
            Nil => OperationUtils.CallBinaryMetamethod(state, a, b, "__pow"),
            _ => throw new UnreachableException()
        };
    
    public static LuaValue FloorDiv(LuaState state, LuaValue a, LuaValue b)
        => MatchOperandTypes(ref a, ref b) switch
        {
            Integer => a._integer / b._integer,
            Float => Math.Floor(a._float / b._float),
            Nil => OperationUtils.CallBinaryMetamethod(state, a, b, "__idiv"),
            _ => throw new UnreachableException()
        };

    public static LuaValue BitwiseAnd(LuaState state, LuaValue a, LuaValue b)
        => MatchOperandTypesInt(ref a, ref b) switch
        {
            Integer => a._integer & b._integer,
            Nil => OperationUtils.CallBinaryMetamethod(state, a, b, "__band"),
            _ => throw new UnreachableException()
        };
    
    public static LuaValue BitwiseOr(LuaState state, LuaValue a, LuaValue b)
        => MatchOperandTypesInt(ref a, ref b) switch
        {
            Integer => a._integer | b._integer,
            Nil => OperationUtils.CallBinaryMetamethod(state, a, b, "__bor"),
            _ => throw new UnreachableException()
        };
    
    public static LuaValue BitwiseXor(LuaState state, LuaValue a, LuaValue b)
        => MatchOperandTypesInt(ref a, ref b) switch
        {
            Integer => a._integer ^ b._integer,
            Nil => OperationUtils.CallBinaryMetamethod(state, a, b, "__bxor"),
            _ => throw new UnreachableException()
        };
    
    // TODO: Bitshift operators

    // TODO: Unary operators, non-arithmetic operations
}
