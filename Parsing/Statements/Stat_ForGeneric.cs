using Ectoplasm.Lexing;
using Ectoplasm.Parsing.Expressions;

namespace Ectoplasm.Parsing.Statements;

/// <param name="namelist">List of loop variables. First is control variable.</param>
/// <param name="explist">Expressions that define loop parameters.</param>
public class Stat_ForGeneric(List<LuaToken> namelist, List<Expression> explist, List<Statement> block, ushort line, 
    ushort col) : Statement(line, col)
{
    
}