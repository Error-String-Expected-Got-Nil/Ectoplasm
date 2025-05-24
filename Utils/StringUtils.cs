using System.Globalization;
using System.Text;

namespace Ectoplasm.Utils;

public static class StringUtils
{
    public static string GetEscapedString(this string str)
    {
        var lit = new StringBuilder(str.Length + 2);
        
        lit.Append('"');
        foreach (var c in str)
            lit.Append(c switch
            {
                '"' => "\\\"",
                '\\' => @"\\",
                '\0' => @"\0",
                '\a' => @"\a",
                '\b' => @"\b",
                '\f' => @"\f",
                '\n' => @"\n",
                '\r' => @"\r",
                '\t' => @"\t",
                '\v' => @"\v",
                _ => char.GetUnicodeCategory(c) == UnicodeCategory.Control 
                    ? @"\u" + ((int)c).ToString("x4") 
                    : c
            });
        lit.Append('"');

        return lit.ToString();
    } 
}