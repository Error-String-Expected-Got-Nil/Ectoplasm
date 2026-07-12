using System.Text;
using Ectoplasm.Parsing.Statements;
using Ectoplasm.Utils;

namespace Ectoplasm.Parsing.Expressions;

// An expression unit which produces an anonymous function object.
public class Expr_FunctionDef(List<string> parameters, bool isVararg, List<Statement> body, string? debugFunctionName, 
    ushort line, ushort col) : Expression(line, col)
{
    public List<string> Parameters => parameters;
    public bool IsVararg => isVararg;
    public List<Statement> Body => body;
    public string? DebugName => debugFunctionName;
    
    internal override void Initialize(Stack<Expression> stack) { }

    public override string ToString()
        => base.ToString() + $" <{debugFunctionName ?? "anonymous"}" +
           $"({string.Join(", ", isVararg ? parameters.Append("...") : parameters)})>";
}
