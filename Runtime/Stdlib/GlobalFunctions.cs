using System.Diagnostics;
using System.Globalization;
using System.Text;
using Ectoplasm.Runtime.Tables;
using Ectoplasm.Runtime.Values;
using Ectoplasm.Utils;

namespace Ectoplasm.Runtime.Stdlib;

public static class GlobalFunctions
{
    private static readonly LuaString StringNil = new("nil"u8);
    private static readonly LuaString StringTrue = new("true"u8);
    private static readonly LuaString StringFalse = new("false"u8);
    private static readonly LuaString StringZero = new("0"u8);

    private static byte[] StringFunction => "function: \0\0\0\0\0\0\0\0"u8.ToArray();
    private static byte[] StringUserdata => "userdata: \0\0\0\0\0\0\0\0"u8.ToArray();
    private static byte[] StringThread => "thread: \0\0\0\0\0\0\0\0"u8.ToArray();
    private static byte[] StringTable => "table: \0\0\0\0\0\0\0\0"u8.ToArray();
    
    public static string LuaToStringUtf16(LuaValue value, bool escapeStrings = false)
        => value.Kind switch
        {
            LuaValueKind.Nil => "nil",
            LuaValueKind.Boolean => value._boolean.ToString(),
            LuaValueKind.Integer => value._integer.ToString(),
            LuaValueKind.Float => value._float.ToString(CultureInfo.InvariantCulture),
            LuaValueKind.String => escapeStrings ? value.StringUtf16Safe.GetEscapedString() : value.StringUtf16Safe,
            // Producing these strings for internal LuaStrings has special handling, but for these I'll assume string
            // interpolation knows what it's doing and can handle this fine.
            LuaValueKind.Function => $"function: {value._ref.GetHashCode():x8}",
            LuaValueKind.Userdata => $"userdata: {value._ref.GetHashCode():x8}",
            LuaValueKind.Thread => $"thread: {value._ref.GetHashCode():x8}",
            LuaValueKind.Table => $"table: {value._ref.GetHashCode():x8}",
            _ => throw new UnreachableException()
        };

    public static LuaValue LuaToString(LuaValue value) => new(LuaToStringInternal(value));
    
    internal static LuaString LuaToStringInternal(LuaValue value)
        => value.Kind switch
        {
            LuaValueKind.Nil => StringNil,
            LuaValueKind.Boolean => value._boolean ? StringTrue : StringFalse,
            LuaValueKind.Integer => LongToString(value._integer),
            // TODO: Native float -> UTF-8 string conversion function
            LuaValueKind.Float => new LuaString(value._float.ToString(CultureInfo.InvariantCulture)),
            LuaValueKind.String => (LuaString)value._ref,
            LuaValueKind.Function => WriteHashUtf8(StringFunction, 10, value._ref),
            LuaValueKind.Userdata => WriteHashUtf8(StringUserdata, 10, value._ref),
            LuaValueKind.Thread => WriteHashUtf8(StringThread, 8, value._ref),
            LuaValueKind.Table => WriteHashUtf8(StringTable, 7, value._ref),
            _ => throw new UnreachableException()
        };

    // Writes the object's hash as an 8-byte UTF-8 hexadecimal sequence into the given byte[], starting at the given
    // index in the array.
    private static LuaString WriteHashUtf8(byte[] str, int index, object obj)
    {
        var hash = obj.GetHashCode();

        // Bit-shifts and masks the hash to select the 4 most significant bits not entered yet, then indexes a UTF-8
        // string to select the appropriate digit for those bits, then adds it to the array at the proper position.
        var j = 0;
        for (var i = 7 * 4; i >= 0; i -= 4, j++)
            str[index + j] = "0123456789abcdef"u8[(hash >> i) & 0b1111];

        return new LuaString(str);
    }

    // TODO: Determine if this is actually better than using long.ToString() and converting to UTF-8
    private static LuaString LongToString(long num)
    {
        if (num == 0) return StringZero;
        
        var isNegative = false;
        if (num < 0)
        {
            num = -num;
            isNegative = true;
        }

        var digits = (int)Math.Log10(num) + 1;

        var str = new byte[digits + (isNegative ? 1 : 0)];
        if (isNegative) str[0] = 45; // 45 is the code for the hyphen '-' in UTF-8

        var modifier = isNegative ? 0 : 1;
        for (var i = digits - modifier; i > 0 - modifier; i--)
        {
            str[i] = "0123456789"u8[(int)(num % 10)];
            num /= 10;
        }

        return new LuaString(str);
    }
}