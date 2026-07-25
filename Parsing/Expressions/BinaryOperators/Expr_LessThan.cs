using System.Reflection.Emit;
using Ectoplasm.Runtime.Values;

namespace Ectoplasm.Parsing.Expressions.BinaryOperators;

// Greater than is implemented as swapping the operands of less than
public class Expr_LessThan(bool swapOperands, ushort line, ushort col) : Expr_Binary(line, col)
{
    public override void Compile(ILGenerator il, Prototype proto)
    {
        il.Emit(OpCodes.Ldarg_1);
        OpA.Compile(il, proto);
        OpB.Compile(il, proto);
        il.Emit(OpCodes.Call, ReflectionRefs.Op_LessThan);
    }
    
    internal override void Initialize(Stack<Expression> stack)
    {
        base.Initialize(stack);

        if (swapOperands) (OpA, OpB) = (OpB, OpA);
    }
}