using System.Diagnostics;
using static Ectoplasm.Runtime.LuaValueKind;
// ReSharper disable SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault

namespace Ectoplasm.Runtime.Values;

/// <summary>
/// Static class holding the implementations for basic arithmetic operations.
/// </summary>
public static class Arithmetic
{
    public static LuaValue Add(LuaState state, LuaValue a, LuaValue b)
        => OperationUtils.MatchOperandTypes(ref a, ref b) switch
        {
            Integer => a._integer + b._integer,
            Float => a._float + b._float,
            Nil => OperationUtils.CallBinaryMetamethod(state, a, b, "__add"),
            _ => throw new UnreachableException()
        };
    
    public static LuaValue Sub(LuaState state, LuaValue a, LuaValue b)
        => OperationUtils.MatchOperandTypes(ref a, ref b) switch
        {
            Integer => a._integer - b._integer,
            Float => a._float - b._float,
            Nil => OperationUtils.CallBinaryMetamethod(state, a, b, "__sub"),
            _ => throw new UnreachableException()
        };
    
    public static LuaValue Mul(LuaState state, LuaValue a, LuaValue b)
        => OperationUtils.MatchOperandTypes(ref a, ref b) switch
        {
            Integer => a._integer * b._integer,
            Float => a._float * b._float,
            Nil => OperationUtils.CallBinaryMetamethod(state, a, b, "__mul"),
            _ => throw new UnreachableException()
        };
    
    public static LuaValue Div(LuaState state, LuaValue a, LuaValue b)
        => OperationUtils.MatchOperandTypes(ref a, ref b) switch
        {
            Integer => (double)a._integer / b._integer,
            Float => a._float / b._float,
            Nil => OperationUtils.CallBinaryMetamethod(state, a, b, "__div"),
            _ => throw new UnreachableException()
        };
    
    public static LuaValue Mod(LuaState state, LuaValue a, LuaValue b)
        => OperationUtils.MatchOperandTypes(ref a, ref b) switch
        {
            Integer => a._integer % b._integer,
            Float => a._float % b._float,
            Nil => OperationUtils.CallBinaryMetamethod(state, a, b, "__mod"),
            _ => throw new UnreachableException()
        };
    
    public static LuaValue Pow(LuaState state, LuaValue a, LuaValue b)
        => OperationUtils.MatchOperandTypes(ref a, ref b) switch
        {
            Integer => Math.Pow(a._integer, b._integer),
            Float => Math.Pow(a._float, b._float),
            Nil => OperationUtils.CallBinaryMetamethod(state, a, b, "__pow"),
            _ => throw new UnreachableException()
        };

    public static LuaValue Neg(LuaState state, LuaValue value)
        => value._kind switch
        {
            Integer => -value._integer,
            Float => -value._float,
            _ => OperationUtils.CallUnaryMetamethod(state, value, "__unm")
        };
    
    public static LuaValue FloorDiv(LuaState state, LuaValue a, LuaValue b)
        => OperationUtils.MatchOperandTypes(ref a, ref b) switch
        {
            Integer => a._integer / b._integer,
            Float => Math.Floor(a._float / b._float),
            Nil => OperationUtils.CallBinaryMetamethod(state, a, b, "__idiv"),
            _ => throw new UnreachableException()
        };

    public static LuaValue BitwiseAnd(LuaState state, LuaValue a, LuaValue b)
        => OperationUtils.MatchOperandTypesInt(ref a, ref b) switch
        {
            Integer => a._integer & b._integer,
            Nil => OperationUtils.CallBinaryMetamethod(state, a, b, "__band"),
            _ => throw new UnreachableException()
        };
    
    public static LuaValue BitwiseOr(LuaState state, LuaValue a, LuaValue b)
        => OperationUtils.MatchOperandTypesInt(ref a, ref b) switch
        {
            Integer => a._integer | b._integer,
            Nil => OperationUtils.CallBinaryMetamethod(state, a, b, "__bor"),
            _ => throw new UnreachableException()
        };
    
    public static LuaValue BitwiseXor(LuaState state, LuaValue a, LuaValue b)
        => OperationUtils.MatchOperandTypesInt(ref a, ref b) switch
        {
            Integer => a._integer ^ b._integer,
            Nil => OperationUtils.CallBinaryMetamethod(state, a, b, "__bxor"),
            _ => throw new UnreachableException()
        };

    public static LuaValue BitwiseNot(LuaState state, LuaValue value)
        => value._kind switch
        {
            Integer => ~value._integer,
            Float => value.TryCoerceInteger(out var intVal) 
                        ? ~intVal 
                        : OperationUtils.CallUnaryMetamethod(state, value, "__bnot"),
            _ => OperationUtils.CallUnaryMetamethod(state, value, "__bnot")
        };

    public static LuaValue BitshiftRight(LuaState state, LuaValue a, LuaValue b)
        => OperationUtils.MatchOperandTypesInt(ref a, ref b) switch
        {
            Integer => OperationUtils.LuaBitshift(a._integer, b._integer),
            Nil => OperationUtils.CallBinaryMetamethod(state, a, b, "__shr"),
            _ => throw new UnreachableException()
        };

    public static LuaValue BitshiftLeft(LuaState state, LuaValue a, LuaValue b)
        => OperationUtils.MatchOperandTypesInt(ref a, ref b) switch
        {
            Integer => OperationUtils.LuaBitshift(a._integer, -b._integer),
            Nil => OperationUtils.CallBinaryMetamethod(state, a, b, "__shl"),
            _ => throw new UnreachableException()
        };
}
