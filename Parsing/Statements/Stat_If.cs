using Ectoplasm.Parsing.Expressions;

namespace Ectoplasm.Parsing.Statements;

/// <param name="clauses">
/// Ordered list of clauses to evaluate. There is always at least one, which will be the initial 'if' clause. All
/// subsequent clauses will be 'elseif' clauses.
/// </param>
/// <param name="elseBlock">
/// Statements to execute if no clauses are satisfied; the 'else' clause. If null, the if statement had no else clause.
/// </param>
public class Stat_If(List<(Expression Condition, List<Statement> Block)> clauses, List<Statement>? elseBlock, 
    ushort line, ushort col) : Statement(line, col)
{
    
}