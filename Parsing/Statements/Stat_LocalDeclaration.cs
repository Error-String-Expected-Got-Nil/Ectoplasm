using Ectoplasm.Lexing;
using Ectoplasm.Parsing.Expressions;

namespace Ectoplasm.Parsing.Statements;

public class Stat_LocalDeclaration(List<(LuaToken Name, LocalAttribute Attribute)> names, List<Expression>? expressions, 
    ushort line, ushort col) : Statement(line, col)
{
    
}