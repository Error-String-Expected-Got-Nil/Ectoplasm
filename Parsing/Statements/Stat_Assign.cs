using Ectoplasm.Parsing.Expressions;

namespace Ectoplasm.Parsing.Statements;

public class Stat_Assign(List<Expression> variables, List<Expression> values, ushort line, ushort col) 
    : Statement(line, col)
{
    
}