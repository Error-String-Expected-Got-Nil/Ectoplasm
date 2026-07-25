using System.Reflection;
using Ectoplasm.Runtime.Functions;

// ReSharper disable InconsistentNaming

namespace Ectoplasm.Runtime.Values;

/// <summary>
/// Static class containing static readonly properties with various reflection objects (like <see cref="MethodInfo"/>,
/// <see cref="PropertyInfo"/>, etc.) for use in IL compilation.
/// </summary>
public static class ReflectionRefs
{
    public static readonly MethodInfo Type_GetTypeFromHandle = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle))!;

    public static readonly MethodInfo MethodInfo_CreateDelegate =
        typeof(MethodInfo).GetMethod(nameof(MethodInfo.CreateDelegate), [typeof(Type), typeof(object)])!;
    
    public static readonly PropertyInfo LuaValue_IsTruthy = typeof(LuaValue).GetProperty(nameof(LuaValue.IsTruthy))!;

    public static readonly MethodInfo LuaValue_Default = typeof(LuaValue).GetMethod(nameof(LuaValue.Default))!;
    
    public static readonly MethodInfo LuaValue_NewBoolean =
        typeof(LuaValue).GetMethod(nameof(LuaValue.New), [typeof(bool)])!;

    public static readonly MethodInfo LuaValue_NewInteger =
        typeof(LuaValue).GetMethod(nameof(LuaValue.New), [typeof(long)])!;
    
    public static readonly MethodInfo LuaValue_NewFloat =
        typeof(LuaValue).GetMethod(nameof(LuaValue.New), [typeof(double)])!;
    
    public static readonly MethodInfo LuaValue_NewString =
        typeof(LuaValue).GetMethod(nameof(LuaValue.New), [typeof(string)])!;

    public static readonly MethodInfo LuaValue_NewFunction =
        typeof(LuaValue).GetMethod(nameof(LuaValue.New), [typeof(LuaFunction)])!;
    
    public static readonly MethodInfo Op_Add = typeof(Arithmetic).GetMethod(nameof(Arithmetic.Add))!;
    public static readonly MethodInfo Op_Sub = typeof(Arithmetic).GetMethod(nameof(Arithmetic.Sub))!;
    public static readonly MethodInfo Op_Mul = typeof(Arithmetic).GetMethod(nameof(Arithmetic.Mul))!;
    public static readonly MethodInfo Op_Div = typeof(Arithmetic).GetMethod(nameof(Arithmetic.Div))!;
    public static readonly MethodInfo Op_Mod = typeof(Arithmetic).GetMethod(nameof(Arithmetic.Mod))!;
    public static readonly MethodInfo Op_Pow = typeof(Arithmetic).GetMethod(nameof(Arithmetic.Pow))!;
    public static readonly MethodInfo Op_Neg = typeof(Arithmetic).GetMethod(nameof(Arithmetic.Neg))!;
    public static readonly MethodInfo Op_FloorDiv = typeof(Arithmetic).GetMethod(nameof(Arithmetic.FloorDiv))!;
    public static readonly MethodInfo Op_BitwiseAnd = typeof(Arithmetic).GetMethod(nameof(Arithmetic.BitwiseAnd))!;
    public static readonly MethodInfo Op_BitwiseOr = typeof(Arithmetic).GetMethod(nameof(Arithmetic.BitwiseOr))!;
    public static readonly MethodInfo Op_BitwiseXor = typeof(Arithmetic).GetMethod(nameof(Arithmetic.BitwiseXor))!;
    public static readonly MethodInfo Op_BitwiseNot = typeof(Arithmetic).GetMethod(nameof(Arithmetic.BitwiseNot))!;
    public static readonly MethodInfo Op_Shr = typeof(Arithmetic).GetMethod(nameof(Arithmetic.BitshiftRight))!;
    public static readonly MethodInfo Op_Shl = typeof(Arithmetic).GetMethod(nameof(Arithmetic.BitshiftLeft))!;

    public static readonly MethodInfo Op_Concat = typeof(Operations).GetMethod(nameof(Operations.Concat))!;
    public static readonly MethodInfo Op_Length = typeof(Operations).GetMethod(nameof(Operations.Length))!;
    public static readonly MethodInfo Op_EqualTo = typeof(Operations).GetMethod(nameof(Operations.EqualTo))!;
    public static readonly MethodInfo Op_LessThan = typeof(Operations).GetMethod(nameof(Operations.LessThan))!;
    public static readonly MethodInfo Op_LessOrEq = typeof(Operations).GetMethod(nameof(Operations.LessThanOrEqualTo))!;
    public static readonly MethodInfo Op_GetIndex = typeof(Operations).GetMethod(nameof(Operations.GetIndex))!;
    public static readonly MethodInfo Op_SetIndex = typeof(Operations).GetMethod(nameof(Operations.SetIndex))!;

    public static readonly FieldInfo Closure_Upvalues = typeof(Closure).GetField(nameof(Closure.Upvalues))!;
    public static readonly FieldInfo Closure_Prototypes = typeof(Closure).GetField(nameof(Closure.Prototypes))!;

    public static readonly ConstructorInfo Closure_Ctor =
        typeof(Closure).GetConstructor([typeof(Upvalue[]), typeof(CompiledPrototype[])])!;

    public static readonly FieldInfo Upvalue_Value = typeof(Upvalue).GetField(nameof(Upvalue.Value))!;

    public static readonly FieldInfo CompiledPrototype_Function =
        typeof(CompiledPrototype).GetField(nameof(CompiledPrototype.Function))!;
    
    public static readonly FieldInfo CompiledPrototype_Prototypes =
        typeof(CompiledPrototype).GetField(nameof(CompiledPrototype.Prototypes))!;
}