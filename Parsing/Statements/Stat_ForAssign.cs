using Ectoplasm.Lexing;
using Ectoplasm.Parsing.Expressions;

namespace Ectoplasm.Parsing.Statements;

/// <param name="name">Token containing the name of the loop control variable.</param>
/// <param name="initial">Expression producing the initial value of the starting variable.</param>
/// <param name="end">Expression producing the value to end the loop on.</param>
/// <param name="increment">Optional expression to increment the control variable with.</param>
public class Stat_ForAssign(LuaToken name, Expression initial, Expression end, Expression? increment, ushort line, 
    ushort col) : Statement(line, col)
{
    
}