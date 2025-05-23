using System.Diagnostics;
// ReSharper disable SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault

namespace Ectoplasm.Runtime.LuaValue;

public readonly partial struct LuaValue
{
    // This file holds the "simple" forms of various arithmetic operations on LuaValues, which do not need access to a 
    // LuaState or use metatables.

    /// <summary>
    /// Ensure two number LuaValues have the same Kind. If either value is not a number
    /// (<see cref="LuaValueKind.Integer"/> or <see cref="LuaValueKind.Float"/>), this throws an exception.
    /// </summary>
    /// <returns>
    /// Either <see cref="LuaValueKind.Integer"/> or <see cref="LuaValueKind.Float"/>. Both LuaValues will be converted
    /// to the returned kind.
    /// </returns>
    public static LuaValueKind PrepareForSimpleArithmetic(ref LuaValue a, ref LuaValue b)
    {
        if (a.Kind is not (LuaValueKind.Integer or LuaValueKind.Float))
            throw new LuaRuntimeException($"Attempt to perform simple arithmetic on non-number kind {a.Kind}");
        if (b.Kind is not (LuaValueKind.Integer or LuaValueKind.Float))
            throw new LuaRuntimeException($"Attempt to perform simple arithmetic on non-number kind {b.Kind}");

        // Easy case: Both values have same kind, can return immediately.
        if (a.Kind == b.Kind) return a.Kind;

        // Kinds are different and 'a' is a float, convert 'b' to float.
        if (a.Kind == LuaValueKind.Float)
        {
            b = new LuaValue((double)b._integer);
            return LuaValueKind.Float;
        }

        // Kinds are different, 'a' is not float, convert 'a' to float.
        a = new LuaValue((double)a._integer);
        return LuaValueKind.Float;
    }

    public static LuaValue SimpleAdd(LuaValue a, LuaValue b)
        => PrepareForSimpleArithmetic(ref a, ref b) switch
        {
            LuaValueKind.Integer => new LuaValue(a._integer + b._integer),
            LuaValueKind.Float => new LuaValue(a._float + b._float),
            _ => throw new UnreachableException()
        };
    
    public static LuaValue SimpleSub(LuaValue a, LuaValue b)
        => PrepareForSimpleArithmetic(ref a, ref b) switch
        {
            LuaValueKind.Integer => new LuaValue(a._integer - b._integer),
            LuaValueKind.Float => new LuaValue(a._float - b._float),
            _ => throw new UnreachableException()
        };
    
    public static LuaValue SimpleMul(LuaValue a, LuaValue b)
        => PrepareForSimpleArithmetic(ref a, ref b) switch
        {
            LuaValueKind.Integer => new LuaValue(a._integer * b._integer),
            LuaValueKind.Float => new LuaValue(a._float * b._float),
            _ => throw new UnreachableException()
        };
    
    public static LuaValue SimpleDiv(LuaValue a, LuaValue b)
        => PrepareForSimpleArithmetic(ref a, ref b) switch
        {
            LuaValueKind.Integer => new LuaValue(a._integer / b._integer),
            LuaValueKind.Float => new LuaValue(a._float / b._float),
            _ => throw new UnreachableException()
        };
    
    public static LuaValue SimpleIntDiv(LuaValue a, LuaValue b)
        => PrepareForSimpleArithmetic(ref a, ref b) switch
        {
            LuaValueKind.Integer => new LuaValue(a._integer / b._integer),
            LuaValueKind.Float => new LuaValue((long)(a._float / b._float)),
            _ => throw new UnreachableException()
        };
}