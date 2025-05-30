namespace Ectoplasm.Parsing.Expressions;

public class Expr_Root(Expression root, ushort line, ushort col) : Expression(line, col)
{
    private bool _init;
    
    // Expr_Root checks if it's already initialized specifically for recursive index expressions. Expr_Index is used for
    // every form of indexing, but in the specific case of indexing like: 'tab[expr]', the 'expr' does not need to be 
    // initialized, since it is parsed recursively. This 'expr' will always be Expr_Root due to recursive parsing.
    internal override void Initialize(Stack<Expression> stack)
    {
        if (_init) return;
        root.Initialize(stack);
        _init = true;
    }
}