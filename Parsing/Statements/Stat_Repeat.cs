using Ectoplasm.Parsing.Expressions;

namespace Ectoplasm.Parsing.Statements;

public class Stat_Repeat(Expression condition, List<Statement> contents, ushort line, ushort col) : Statement(line, col)
{
    
}