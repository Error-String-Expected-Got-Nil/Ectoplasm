using System.Diagnostics;
using System.Reflection.Emit;

namespace Ectoplasm.Utils;

// ReSharper disable once InconsistentNaming
public static class ILUtils
{
    /// <summary>
    /// Emit a <c>ldc.i4</c> instruction for loading an integer constant onto the stack, using the shortest form
    /// possible.
    /// </summary>
    public static void LoadConstant(this ILGenerator il, int val)
    {
        switch (val)
        {
            case >= -1 and <= 8:
                il.Emit(val switch
                {
                    -1 => OpCodes.Ldc_I4_M1,
                    0 => OpCodes.Ldc_I4_0,
                    1 => OpCodes.Ldc_I4_1,
                    2 => OpCodes.Ldc_I4_2,
                    3 => OpCodes.Ldc_I4_3,
                    4 => OpCodes.Ldc_I4_4,
                    5 => OpCodes.Ldc_I4_5,
                    6 => OpCodes.Ldc_I4_6,
                    7 => OpCodes.Ldc_I4_7,
                    8 => OpCodes.Ldc_I4_8,
                    _ => throw new UnreachableException()
                });
                return;
            case >= sbyte.MinValue and <= sbyte.MaxValue:
                il.Emit(OpCodes.Ldc_I4_S, (sbyte)val);
                return;
            default:
                il.Emit(OpCodes.Ldc_I4, val);
                break;
        }
    }
}