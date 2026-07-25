using System.Reflection.Emit;

namespace Ectoplasm.Parsing.Expressions;

public class Expr_Root(ushort line, ushort col) : Expression(line, col)
{
    private Expression? _root;
    private bool _init;

    public override bool IsAssignable => _root!.IsAssignable;
    public override bool IsCall => _root!.IsCall;
    public override bool IsVariadic => _root!.IsVariadic;

    /// <summary>
    /// If this expression consists only of an Expr_Variable, returns the Name used for that variable as a string.
    /// Otherwise, returns null.
    /// </summary>
    public string? Name => _root is Expr_Variable name ? name.Name : null;

    public override void Compile(ILGenerator il, Prototype proto) => _root!.Compile(il, proto);

    public override void CompileVariadic(ILGenerator il, Prototype proto) => _root!.CompileVariadic(il, proto);

    public override void CompileAssign(ILGenerator il, Prototype p, Action vp) => _root!.CompileAssign(il, p, vp);

    // Expr_Root checks if it's already initialized specifically for recursive index expressions. Expr_Index is used for
    // every form of indexing, but in the specific case of indexing like: 'tab[expr]', the 'expr' does not need to be 
    // initialized, since it is parsed recursively. This 'expr' will always be Expr_Root due to recursive parsing.
    internal override void Initialize(Stack<Expression> stack)
    {
        if (_init) return;
        _root = stack.Pop();
        _root.Initialize(stack);
        _init = true;
    }

    // Expr_Root skips itself when enumerating
    public override IEnumerable<(Expression Expr, int Depth)> DepthFirstEnumerate(int depth = 0)
        => _root!.DepthFirstEnumerate(depth);
}