using System.Diagnostics;
using System.Globalization;
using System.Text;
using Ectoplasm.Runtime.Tables;
using Ectoplasm.Runtime.Values;
using Ectoplasm.Utils;

namespace Ectoplasm.Runtime.Stdlib;

public static class GlobalFunctions
{
    public static string LuaToString(LuaValue value, bool escapeStrings = false)
        => value.Kind switch
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
}