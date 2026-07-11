using System.Diagnostics;
using System.Text;
using Ectoplasm.Lexing;
using Ectoplasm.Parsing.Expressions;
using Ectoplasm.Utils;

namespace Ectoplasm.Parsing.Statements;

public class Stat_LocalDeclaration(List<(LuaToken Name, LocalAttribute Attribute)> names, List<Expression>? expressions, 
    ushort line, ushort col) : Statement(line, col)
{
    protected override void AddToDebugString(StringBuilder str, int depth)
    {
        base.AddToDebugString(str, depth);
        for (var i = 0; i < names.Count; i++)
        {
            var (name, attr) = names[i];
            str.AppendRep(".   ", depth + 1,
                $"{(string)name.Data!}{attr switch {
                    LocalAttribute.None => "",
                    LocalAttribute.Close => "<close>",
                    LocalAttribute.Const => "<const>",
                    _ => throw new UnreachableException()
                }}");
            
            if (expressions is null) continue;
            
            if (expressions.Count > i) expressions[i].AddToDebugString(str, depth + 2);
            else str.AppendRep(".   ", depth + 2, "undefined");
        }
    }
}