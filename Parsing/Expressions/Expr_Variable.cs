namespace Ectoplasm.Parsing.Expressions;

public class Expr_Variable(string name, ushort line, ushort col) : Expression(line, col)
{
    public string Name => name;

    public override bool IsAssignable => true;

    /// <summary>
    /// If true, this expression refers to a global variable rather than a local variable.
    /// </summary>
    public bool IsGlobal;
    
    /// <summary>
    /// The local variable object this expression takes its value from when resolved. If null and <see cref="IsGlobal"/>
    /// is false, this expression hasn't been analyzed yet. If null and <see cref="IsGlobal"/> is true, this refers to a
    /// global variable to be indexed from the global environment table.
    /// </summary>
    public LocalVariable? Source;

    internal override void Initialize(Stack<Expression> stack) { }

    public override string ToString() => base.ToString() + $" <{name}>";
}