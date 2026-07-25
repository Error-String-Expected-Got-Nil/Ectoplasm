using System.Reflection.Emit;
using Ectoplasm.Runtime.Values;
using Ectoplasm.Utils;

namespace Ectoplasm.Parsing.Expressions;

public class Expr_Variable(string name, ushort line, ushort col) : Expression(line, col)
{
    public string Name => name;

    public override bool IsAssignable => Source?.Attribute is not (LocalAttribute.Const or LocalAttribute.Close);

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

    public override void Compile(ILGenerator il, Prototype proto)
    {
        if (IsGlobal)
        {
            // Global variables are indexed from the global environment, which is (normally) always the first upvalue
            // of any function.
            il.Emit(OpCodes.Ldarg_1); // LuaState
            il.Emit(OpCodes.Ldarg_0); // Closure
            il.Emit(OpCodes.Ldfld, ReflectionRefs.Closure_Upvalues); // Load upvalues array
            il.Emit(OpCodes.Ldc_I4_0); // First element
            il.Emit(OpCodes.Ldelem); // Index it, _ENV upvalue is now on top of stack
            il.Emit(OpCodes.Ldfld, ReflectionRefs.Upvalue_Value); // Load its value 
            il.Emit(OpCodes.Ldstr, name); // Load variable name
            il.Emit(OpCodes.Call, ReflectionRefs.LuaValue_NewString); // Convert to LuaValue
            il.Emit(OpCodes.Call, ReflectionRefs.Op_GetIndex); // Index from the global table
            return;
        }

        if (Source is null) throw new InvalidOperationException("Attempt to compile unresolved variable");

        if (Source.IsUpvalue)
        {
            if (Source.ExternalSource is not null)
            {
                // Value is in the closure's upvalues array.
                il.Emit(OpCodes.Ldarg_0); // Closure
                il.Emit(OpCodes.Ldfld, ReflectionRefs.Closure_Upvalues); // Load upvalues array
                il.LoadConstant(Source.Index);
                il.Emit(OpCodes.Ldelem); // Load the upvalue
            }
            else
            {
                // Value is in the function locals.
                il.LoadLocal((ushort)Source.Index);
            }
            // Upvalue object is now on top of stack.
            il.Emit(OpCodes.Ldfld, ReflectionRefs.Upvalue_Value);
            return;
        }
        
        // Value is not an upvalue, it's a regular local variable.
        il.LoadLocal((ushort)Source.Index);
    }

    public override void CompileAssign(ILGenerator il, Prototype proto, Action valueProducer)
    {
        if (IsGlobal)
        {
            il.Emit(OpCodes.Ldarg_1); // LuaState
            il.Emit(OpCodes.Ldarg_0); // Closure
            il.Emit(OpCodes.Ldfld, ReflectionRefs.Closure_Upvalues); // Load upvalues array
            il.Emit(OpCodes.Ldc_I4_0); // First element
            il.Emit(OpCodes.Ldelem); // Index it, _ENV upvalue is now on top of stack
            il.Emit(OpCodes.Ldfld, ReflectionRefs.Upvalue_Value); // Load its value 
            il.Emit(OpCodes.Ldstr, name); // Load variable name
            il.Emit(OpCodes.Call, ReflectionRefs.LuaValue_NewString); // Convert to LuaValue
            valueProducer();
            il.Emit(OpCodes.Call, ReflectionRefs.Op_SetIndex);
            return;
        }
        
        if (Source is null) throw new InvalidOperationException("Attempt to compile unresolved variable");
        
        if (Source.IsUpvalue)
        {
            if (Source.ExternalSource is not null)
            {
                il.Emit(OpCodes.Ldarg_0); // Closure
                il.Emit(OpCodes.Ldfld, ReflectionRefs.Closure_Upvalues); // Load upvalues array
                il.LoadConstant(Source.Index);
                il.Emit(OpCodes.Ldelem); // Load the upvalue
            }
            else
            {
                il.LoadLocal((ushort)Source.Index);
            }

            valueProducer();
            il.Emit(OpCodes.Stfld, ReflectionRefs.Upvalue_Value);
            return;
        }

        valueProducer();
        il.StoreLocal((ushort)Source.Index);
    }

    internal override void Initialize(Stack<Expression> stack) { }

    public override string ToString() => base.ToString() + $" <{name}>";
}