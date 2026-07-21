using static Ectoplasm.Runtime.LuaValueKind;

namespace Ectoplasm.Runtime.Values;

/// <summary>
/// Static class holding the implementations for basic arithmetic operations.
/// </summary>
public static class Arithmetic
{
    /// <summary>
    /// Takes two Lua values and attempts to match their types for standard arithmetic operations like addition. Returns the type both were
    /// coerced to, or <see cref="Nil"/> if a coercion could not be made (and metatables must be checked).
    /// </summary>
    private static LuaValueKind MatchOperandTypes(ref LuaValue a, ref LuaValue b)
    {
        // Operands are not both numbers, cannot coerce them.
        if (!(a._kind is Integer or Float && b._kind is Integer or Float)) return Nil;

        // Types match, no coercion necessary.
        if (a._kind is Integer && b._kind is Integer) return Integer;
        if (a._kind is Float && b._kind is Float) return Float;

        // Operand types are different but are either integer or float.
        // First is integer, so second is float, have to cast first.
        if (a._kind is Integer)
        {
            a = new LuaValue() { _kind = Float, _float = (double)a._integer };
            return Float;
        }

        // First must be float and the second integer, cast second.
        b = new LuaValue() { _kind = Float, _float = (double)b._integer };
        return Float;
    }
}
