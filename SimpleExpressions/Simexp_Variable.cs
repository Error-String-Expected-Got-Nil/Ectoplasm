using System.Reflection;
using System.Reflection.Emit;
using Ectoplasm.Runtime.Values;
using Ectoplasm.Runtime.Tables;
using Ectoplasm.Utils;

namespace Ectoplasm.SimpleExpressions;

public class Simexp_Variable(LuaValue index, int upvalueIndex) : SimpleExpression
{
    // Only indexer properties have parameters, and LuaTable should only ever have one indexer, so this is good enough
    // to get the index getter for LuaTables.
    private static readonly MethodInfo LuaTableGetItemInfo
        = typeof(LuaTable).GetProperties().First(prop => prop.GetIndexParameters().Length > 0).GetMethod!;
    
    /// <inheritdoc/>
    public override bool IsInit => true;

    /// <inheritdoc/>
    public override LuaValue Evaluate(LuaTable? env) => env?[index] ?? default;

    /// <inheritdoc/>
    public override void Init(Stack<SimpleExpression> stack) { }
    
    /// <inheritdoc/>
    public override void Compile(ILGenerator il)
    {
        il.Emit(OpCodes.Ldarg_1); // Load env table
        il.Emit(OpCodes.Ldarg_0); // Load upvalue array
        il.LoadConstant(upvalueIndex); // Load upvalue index
        il.Emit(OpCodes.Ldelem, typeof(LuaValue)); // Load upvalue
        il.Emit(OpCodes.Callvirt, LuaTableGetItemInfo); // Get item from table
    }
}