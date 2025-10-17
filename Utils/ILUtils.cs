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
                return;
        }
    }

    /// <summary>
    /// <para>
    /// Emit a <c>ldc.i4</c> instruction for loading an integer constant onto the stack, using the shortest form
    /// possible.
    /// </para>
    /// <para>
    /// Signed and unsigned integers are distinguished by usage in IL. This is a simple alias that uses an unchecked
    /// cast to type-pun the unsigned integer to a signed one, then calls <see cref="LoadConstant(ILGenerator,int)"/>.
    /// </para>
    /// </summary>
    public static void LoadConstant(this ILGenerator il, uint val)
        => LoadConstant(il, unchecked((int)val));

    /// <summary>
    /// Emit a <c>ldarg</c> instruction for loading a method argument onto the stack, using the shortest form possible.
    /// </summary>
    public static void LoadArgument(this ILGenerator il, ushort index)
    {
        switch (index)
        {
            case <= 3:
                il.Emit(index switch
                {
                    0 => OpCodes.Ldarg_0,
                    1 => OpCodes.Ldarg_1,
                    2 => OpCodes.Ldarg_2,
                    3 => OpCodes.Ldarg_3,
                    _ => throw new UnreachableException()
                });
                return;
            case <= byte.MaxValue:
                il.Emit(OpCodes.Ldarg_S, (byte)index);
                return;
            default:
                il.Emit(OpCodes.Ldarg, index);
                return;
        }
    }
    
    /// <summary>
    /// Emit a <c>starg</c> instruction for storing a value on the stack to a method argument, using the shortest form
    /// possible.
    /// </summary>
    public static void StoreArgument(this ILGenerator il, ushort index)
    {
        switch (index)
        {
            case <= byte.MaxValue:
                il.Emit(OpCodes.Starg_S, (byte)index);
                return;
            default:
                il.Emit(OpCodes.Starg, index);
                return;
        }
    }
    
    /// <summary>
    /// Emit a <c>ldloc</c> instruction for loading a local variable onto the stack, using the shortest form possible.
    /// </summary>
    public static void LoadLocal(this ILGenerator il, ushort index)
    {
        switch (index)
        {
            case <= 3:
                il.Emit(index switch
                {
                    0 => OpCodes.Ldloc_0,
                    1 => OpCodes.Ldloc_1,
                    2 => OpCodes.Ldloc_2,
                    3 => OpCodes.Ldloc_3,
                    _ => throw new UnreachableException()
                });
                return;
            case <= byte.MaxValue:
                il.Emit(OpCodes.Ldloc_S, (byte)index);
                return;
            // Strictly speaking, an index of ushort.MaxValue is not permitted, but in practice it's unlikely that
            // anything will actually try to load a local with that high of an index. If it does, we'll let other
            // stuff catch that error.
            default:
                il.Emit(OpCodes.Ldloc, index);
                return;
        }
    }
    
    /// <summary>
    /// Emit a <c>stloc</c> instruction for storing a value on the stack to a local variable, using the shortest form
    /// possible.
    /// </summary>
    public static void StoreLocal(this ILGenerator il, ushort index)
    {
        switch (index)
        {
            case <= 3:
                il.Emit(index switch
                {
                    0 => OpCodes.Stloc_0,
                    1 => OpCodes.Stloc_1,
                    2 => OpCodes.Stloc_2,
                    3 => OpCodes.Stloc_3,
                    _ => throw new UnreachableException()
                });
                return;
            case <= byte.MaxValue:
                il.Emit(OpCodes.Stloc_S, (byte)index);
                return;
            default:
                il.Emit(OpCodes.Stloc, index);
                return;
        }
    }
}