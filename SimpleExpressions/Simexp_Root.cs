using System.Reflection.Emit;
using System.Text;
using Ectoplasm.Utils;
using Ectoplasm.Runtime.Values;
using Ectoplasm.Runtime.Tables;

namespace Ectoplasm.SimpleExpressions;

public class Simexp_Root(SimpleExpression start, LuaValue[] upvalues) : SimpleExpression
{
    private bool _init;
    /// <inheritdoc/>
    public override bool IsInit => _init;

    /// <inheritdoc/>
    public override LuaValue Evaluate(LuaTable? env) => start.Evaluate(env);

    /// <inheritdoc/>
    public override void Init(Stack<SimpleExpression> stack)
    {
        if (!start.IsInit) start.Init(stack);
        _init = true;
    }

    /// <inheritdoc/>
    public override void Compile(ILGenerator il) => start.Compile(il);

    /// <summary>
    /// Compiles this expression into IL stored in a <see cref="DynamicMethod"/>, which is converted to a delegate and
    /// returned.
    /// </summary>
    /// <returns>A <see cref="SimpleExpressionDelegate"/> which evaluates this expression.</returns>
    public SimpleExpressionDelegate MakeDelegate()
    {
        var dyn = new DynamicMethod("", typeof(LuaValue), [typeof(LuaValue).MakeArrayType(), typeof(LuaTable)]);
        var il = dyn.GetILGenerator();
        start.Compile(il);
        il.Emit(OpCodes.Ret);

        return dyn.CreateDelegate<SimpleExpressionDelegate>(upvalues);
    }

    /// <summary>
    /// Compiles this expression into IL stored in a <see cref="DynamicMethod"/>, which is converted to a delegate and
    /// returned. Also logs generated IL.
    /// </summary>
    /// <param name="log">Out parameter containing a log of generated IL.</param>
    /// <returns>A <see cref="SimpleExpressionDelegate"/> which evaluates this expression.</returns>
    public SimpleExpressionDelegate MakeDelegateWithLogging(out string log)
    {
        var dyn = new DynamicMethod("", typeof(LuaValue), [typeof(LuaValue).MakeArrayType(), typeof(LuaTable)]);
        var il = new LoggedILGenerator(dyn.GetILGenerator());
        start.Compile(il);
        il.Emit(OpCodes.Ret);
        
        log = new StringBuilder()
            .Append('[')
            .Append(string.Join(", ", upvalues))
            .Append(']')
            .AppendLine()
            .Append(il.GetLog())
            .ToString();
        
        return dyn.CreateDelegate<SimpleExpressionDelegate>(upvalues);
    }
}