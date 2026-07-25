using System.Reflection.Emit;
using System.Text;
using Ectoplasm.Parsing.Statements;
using Ectoplasm.Runtime;
using Ectoplasm.Runtime.Functions;
using Ectoplasm.Runtime.Values;
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
    /// Indicates whether this expression may return more than one value. If true, it is permissible to call
    /// <see cref="CompileVariadic"/>.
    /// </summary>
    public virtual bool IsVariadic => false;

    /// <summary>
    /// <para>
    /// Accepts an ILGenerator and emits IL code which evaluates this expression. This always has a stack transition
    /// behavior of pushing one <see cref="LuaValue"/>.
    /// </para>
    /// <para>
    /// It is assumed to always be the case that the provided <see cref="ILGenerator"/> is being used to generate a
    /// <see cref="LuaFunction"/> instance delegate, and therefore the first argument is a <see cref="Closure"/>, and
    /// the second argument is a <see cref="LuaState"/>.
    /// </para>
    /// </summary>
    /// <param name="il">The <see cref="ILGenerator"/> to emit code in.</param>
    /// <param name="proto">The function prototype this expression is inside.</param>
    public abstract void Compile(ILGenerator il, Prototype proto);

    /// <summary>
    /// <para>
    /// Similar to <see cref="Compile"/>, except instead of pushing a single value to the IL stack, this pushes all
    /// results of this expression to the <see cref="LuaState"/> stack and increments <see cref="LuaState.StackTop"/>
    /// accordingly.
    /// </para>
    /// <para>
    /// It is only permissible to call this if <see cref="IsVariadic"/> is true, otherwise this will throw an exception.
    /// </para>
    /// </summary>
    public virtual void CompileVariadic(ILGenerator il, Prototype proto) => throw new InvalidOperationException();
    
    /// <summary>
    /// <para>
    /// Similar to <see cref="Compile"/>, except this method has neutral stack transition behavior, and accepts an
    /// <see cref="Action"/>, which should push one <see cref="LuaValue"/> to the IL stack which will be assigned to
    /// the variable/location referenced by this expression. In the <see cref="Action"/>, use the same
    /// <see cref="ILGenerator"/> passed to this function.
    /// </para>
    /// <para>
    /// It is only permissible to call this if <see cref="IsAssignable"/> is true, otherwise this will throw an
    /// exception.
    /// </para>
    /// </summary>
    public virtual void CompileAssign(ILGenerator il, Prototype proto, Action valueProducer) 
        => throw new InvalidOperationException();
    
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