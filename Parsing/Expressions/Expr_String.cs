using System.Reflection.Emit;
using Ectoplasm.Runtime.Values;
using Ectoplasm.Utils;

namespace Ectoplasm.Parsing.Expressions;

public class Expr_String(string value, ushort line, ushort col) : Expression(line, col)
{
    public override void Compile(ILGenerator il, Prototype proto)
    {   
        il.Emit(OpCodes.Ldstr, value);
        il.Emit(OpCodes.Call, ReflectionRefs.LuaValue_NewString);
    }

    internal override void Initialize(Stack<Expression> stack) { }

    public override string ToString() => base.ToString() + $" <{value.GetEscapedString()}>";
}