using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using Ectoplasm.Runtime.Values;
using Ectoplasm.Utils;

namespace Ectoplasm.Runtime.Stdlib;

public static class GlobalFunctions
{
    public static string LuaToString(LuaValue value, bool escapeStrings = false)
        => value._kind switch
        {
            LuaValueKind.Nil => "nil",
            LuaValueKind.Boolean => value._boolean.ToString(),
            LuaValueKind.Integer => value._integer.ToString(),
            LuaValueKind.Float => value._float.ToString(CultureInfo.InvariantCulture),
            LuaValueKind.String => escapeStrings ? value.String.GetEscapedString() : value.String,
            LuaValueKind.Function => $"function: {value._ref.GetHashCode():x8}",
            LuaValueKind.Userdata => $"userdata: {value._ref.GetHashCode():x8}",
            LuaValueKind.Thread => $"thread: {value._ref.GetHashCode():x8}",
            LuaValueKind.Table => $"table: {value._ref.GetHashCode():x8}",
            _ => throw new UnreachableException()
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LuaValueEquality(LuaValue a, LuaValue b) 
        => a._kind == b._kind && a._kind switch 
        { 
            LuaValueKind.Nil => true,
            LuaValueKind.Boolean => a._boolean == b._boolean,
            LuaValueKind.Integer => a._integer == b._integer,
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            LuaValueKind.Float => a._float == b._float,
            // All remaining value kinds are reference types, so we can just compare the reference field.
            _ => a._ref == b._ref
        };
}