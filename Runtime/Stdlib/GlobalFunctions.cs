using System.Diagnostics;
using System.Globalization;
using System.Text;
using Ectoplasm.Runtime.Values;
using Ectoplasm.Utils;

namespace Ectoplasm.Runtime.Stdlib;

public static class GlobalFunctions
{
    private static readonly LuaString StringNil = new("nil"u8);
    private static readonly LuaString StringTrue = new("true"u8);
    private static readonly LuaString StringFalse = new("false"u8);
    
    public static string LuaToStringUtf16(LuaValue value, bool escapeStrings = false)
        => value.Kind switch
        {
            LuaValueKind.Nil => "nil",
            LuaValueKind.Boolean => value._boolean.ToString(),
            LuaValueKind.Integer => value._integer.ToString(),
            LuaValueKind.Float => value._float.ToString(CultureInfo.InvariantCulture),
            LuaValueKind.String => escapeStrings ? value.StringUtf16Safe.GetEscapedString() : value.StringUtf16Safe,
            LuaValueKind.Function => $"function 0x{value._ref.GetHashCode():x8}",
            LuaValueKind.Userdata => $"userdata 0x{value._ref.GetHashCode():x8}",
            LuaValueKind.Thread => $"thread 0x{value._ref.GetHashCode():x8}",
            LuaValueKind.Table => $"table 0x{value._ref.GetHashCode():x8}",
            _ => throw new UnreachableException()
        };

    public static LuaValue LuaToString(LuaValue value) => new(LuaToStringInternal(value));
    
    internal static LuaString LuaToStringInternal(LuaValue value)
        => value.Kind switch
        {
            LuaValueKind.Nil => StringNil,
            LuaValueKind.Boolean => value._boolean ? StringTrue : StringFalse,
            // TODO: Slightly inefficient to convert to 'string' then to LuaValue like this, maybe make native integer
            //  and float tostring functions for UTF-8 byte arrays?
            LuaValueKind.Integer => new LuaString(value._integer.ToString()),
            LuaValueKind.Float => new LuaString(value._float.ToString(CultureInfo.InvariantCulture)),
            LuaValueKind.String => (LuaString)value._ref,
            LuaValueKind.Function => new LuaString(Encoding.UTF8.GetBytes($"function 0x{value._ref.GetHashCode():x8}")),
            LuaValueKind.Userdata => new LuaString(Encoding.UTF8.GetBytes($"userdata 0x{value._ref.GetHashCode():x8}")),
            LuaValueKind.Thread => new LuaString(Encoding.UTF8.GetBytes($"thread 0x{value._ref.GetHashCode():x8}")),
            LuaValueKind.Table => new LuaString(Encoding.UTF8.GetBytes($"table 0x{value._ref.GetHashCode():x8}")),
            _ => throw new UnreachableException()
        };
}