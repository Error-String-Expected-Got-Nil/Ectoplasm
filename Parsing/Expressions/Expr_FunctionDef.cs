using Ectoplasm.Parsing.Statements;

namespace Ectoplasm.Parsing.Expressions;

// An expression unit which produces an anonymous function object.
public class Expr_FunctionDef(List<string> parameters, bool isVararg, List<Statement> body, string? debugFunctionName, 
    ushort line, ushort col) : Expression(line, col)
{
    internal override void Initialize(Stack<Expression> stack) { }
}
