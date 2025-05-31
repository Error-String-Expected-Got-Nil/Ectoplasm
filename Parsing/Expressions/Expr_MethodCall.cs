namespace Ectoplasm.Parsing.Expressions;

public class Expr_MethodCall(int argc, ushort line, ushort col) : Expression(line, col)
{
    // Note that arguments will be in this list in reverse order; the first argument is the last in the list.
    private readonly List<Expression> _arguments = [];
    private Expression? _instance;
    private Expression? _functionName;

    internal override void Initialize(Stack<Expression> stack)
    {
        // Function call arguments don't need to be initialized since they are parsed recursively.
        for (var i = 0; i < argc; i++)
            _arguments.Add(stack.Pop());

        var expr = stack.Pop();
        if (expr is not Expr_Index index)
            throw new LuaParsingException("Method call operation expected called operand to be index operation", 
                StartLine, StartCol);
        
        index.Initialize(stack);

        _instance = index.OpA;
        _functionName = index.OpB;
    }

    public override IEnumerable<(Expression Expr, int Depth)> DepthFirstEnumerate(int depth = 0)
        => base.DepthFirstEnumerate(depth)
            .Concat(_arguments
                .Select(arg => arg.DepthFirstEnumerate(depth + 1))
                .Reverse()
                .Aggregate(new List<(Expression Expr, int Depth)>(),
                    (accum, item) =>
                    {
                        accum.AddRange(item);
                        return accum;
                    }))
            .Concat(_instance!.DepthFirstEnumerate(depth + 1))
            .Concat(_functionName!.DepthFirstEnumerate(depth + 1));
}