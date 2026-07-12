using System.Text;
using Ectoplasm.Parsing.Statements;
using Ectoplasm.Utils;

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
    public string GetDebugString(int baseDepth = 0)
        => AddToDebugString(new StringBuilder(), baseDepth).ToString();

    /// <summary>
    /// Adds a debug-formatted printout of this expression tree to the end of a StringBuilder.
    /// </summary>
    public StringBuilder AddToDebugString(StringBuilder str, int baseDepth = 0)
    {
        foreach (var (expr, depth) in DepthFirstEnumerate())
        {
            str.AppendRep(".   ", depth + baseDepth) 
                .Append(expr)
                .AppendLine();

            if (expr is not Expr_FunctionDef func) continue;

            Statement.AddBlockDebugString(str, func.Body, depth + baseDepth + 1);
        }

        return str;
    }
}