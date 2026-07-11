using System.Text;

namespace Ectoplasm.Parsing.Expressions;

public abstract class Expression(ushort line, ushort col)
{
    /// <summary>
    /// The line of source this expression starts on.
    /// </summary>
    public ushort StartLine => line;
    
    /// <summary>
    /// The column of source this expression starts on.
    /// </summary>
    public ushort StartCol => col;

    /// <summary>
    /// Indicates whether this expression is assignable. If true, it is possible to resolve this expression as a
    /// location to which a value can be assigned, instead of just a value.
    /// </summary>
    public virtual bool IsAssignable => false;

    /// <summary>
    /// Indicates whether this expression is a call. If true, it is possible to compile this expression as a standalone
    /// call statement.
    /// </summary>
    public virtual bool IsCall => false;

    /// <summary>
    /// Initialize this expression by popping operands from a given stack and initializing them.
    /// </summary>
    internal abstract void Initialize(Stack<Expression> stack);

    // TODO: Should have a virtual function that returns the assignment location for assignable expressions?
    //  Same for call.
    
    /// <summary>
    /// Returns every member of this expression tree in depth-first order, and the depth of each of them.
    /// </summary>
    public virtual IEnumerable<(Expression Expr, int Depth)> DepthFirstEnumerate(int depth = 0)
    {
        yield return (this, depth);
    }

    public override string ToString() => $"{GetType().Name} [{StartLine}, {StartCol}]";

    /// <summary>
    /// Converts this expression tree to a string in a human-friendly format suitable for debug printouts.
    /// </summary>
    public string GetDebugString()
    {
        var str = new StringBuilder();
        foreach (var (expr, depth) in DepthFirstEnumerate())
        {
            for (var i = 0; i < depth; i++) str.Append(".   ");
            
            str.Append(expr)
                .AppendLine();
        }

        return str.ToString();
    }
}