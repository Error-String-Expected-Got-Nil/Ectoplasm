namespace Ectoplasm.Parsing.Expressions;

public class Expr_Call(int argc, ushort line, ushort col) : Expression(line, col)
{
    // Note that arguments will be in this list in reverse order: The first argument is the last in the list.
    private readonly List<Expression> _arguments = [];
    private Expression? _function;

    internal override void Initialize(Stack<Expression> stack)
    {
        // Function call arguments don't need to be initialized since they are parsed recursively.
        for (var i = 0; i < argc; i++)
            _arguments.Add(stack.Pop());

        _function = stack.Pop();
        _function.Initialize(stack);
    }
}