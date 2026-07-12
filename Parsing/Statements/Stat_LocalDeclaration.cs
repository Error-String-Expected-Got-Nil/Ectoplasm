using System.Diagnostics;
using System.Text;
using Ectoplasm.Lexing;
using Ectoplasm.Parsing.Expressions;
using Ectoplasm.Utils;

namespace Ectoplasm.Parsing.Statements;

// TODO: Important note about upvalues! Each time a local is declared, the corresponding local variable must be 
//  *overwritten*, even if an upvalue. See the reference manual section 3.5 for an example.
public class Stat_LocalDeclaration(List<(string Name, LocalAttribute Attribute)> names, List<Expression>? expressions, 
    ushort line, ushort col) : Statement(line, col)
{
    protected override void AddToDebugString(StringBuilder str, int depth)
    {
        base.AddToDebugString(str, depth);
        for (var i = 0; i < names.Count; i++)
        {
            var (name, attr) = names[i];
            str.AppendRep(".   ", depth + 1,
                $"{name}{attr switch {
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