using System.Diagnostics;
using System.Globalization;
using Ectoplasm.Runtime.Values;

namespace Ectoplasm.Runtime.Stdlib;

public static class GlobalFunctions
{
    public static string LuaToString(LuaValue value)
        => value.Kind switch
        {
            LuaValueKind.Nil => "nil",
            LuaValueKind.Boolean => value._boolean.ToString(),
            LuaValueKind.Integer => value._integer.ToString(),
            LuaValueKind.Float => value._float.ToString(CultureInfo.InvariantCulture),
            LuaValueKind.String => value.StringUtf16Safe,
            LuaValueKind.Function => "<function>",
            LuaValueKind.Userdata => "<userdata>",
            LuaValueKind.Thread => "<thread>",
            LuaValueKind.Table => "<table>",
            _ => throw new UnreachableException()
        };
}