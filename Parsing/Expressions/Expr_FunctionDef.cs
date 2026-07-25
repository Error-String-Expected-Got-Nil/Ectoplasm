using System.Reflection.Emit;
using System.Text;
using Ectoplasm.Parsing.Statements;
using Ectoplasm.Runtime.Functions;
using Ectoplasm.Runtime.Values;
using Ectoplasm.Utils;

namespace Ectoplasm.Parsing.Expressions;

// An expression unit which produces an anonymous function object.
public class Expr_FunctionDef(List<string> parameters, bool isVararg, List<Statement> body, 
    string? debugFunctionName, ushort line, ushort col) : Expression(line, col)
{
    public List<string> Parameters => parameters;
    public bool IsVararg => isVararg;
    public List<Statement> Body => body;
    public string? DebugName => debugFunctionName;

    /// <summary>
    /// The analyzed function prototype this function definition expression uses.
    /// </summary>
    public Prototype? Prototype;

    public override void Compile(ILGenerator il, Prototype proto)
    {
        if (Prototype is null) throw new InvalidOperationException("Attempt to compile unresolved closure");
        var index = Prototype.Parent!.Children.IndexOf(Prototype);
        if (index == -1) throw new InvalidOperationException("Failed to find prototype index");
        index++; // See doc comment on Prototype.Children

        il.Emit(OpCodes.Ldarg_0); // Load closure
        il.Emit(OpCodes.Ldfld, ReflectionRefs.Closure_Prototypes);
        il.LoadConstant(index);
        il.Emit(OpCodes.Ldelem); // Load the CompiledPrototype that refers to this function definition
        il.Emit(OpCodes.Ldfld, ReflectionRefs.CompiledPrototype_Function); // Load the MethodInfo for it
        il.Emit(OpCodes.Ldtoken, typeof(LuaFunction));
        il.Emit(OpCodes.Call, ReflectionRefs.Type_GetTypeFromHandle); // Load LuaFunction type
        
        // Now we need to generate the closure object
        il.LoadConstant(Prototype.Externals.Count);
        il.Emit(OpCodes.Newarr, typeof(Upvalue)); // Upvalues array

        for (var i = 0; i < Prototype.Externals.Count; i++)
        {
            il.Emit(OpCodes.Dup); // Duplicate upvalues array ref
            il.LoadConstant(i); // Push index
            // src is the LocalVariable in the parent prototype, which is also the context this is being compiled in.
            // This will always have IsUpvalue == true.
            var src = Prototype.Externals[i].ExternalSource!;
            if (src.ExternalSource is not null)
            {
                // Need to get from upvalues array.
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, ReflectionRefs.Closure_Upvalues);
                il.LoadConstant(src.Index);
                il.Emit(OpCodes.Ldelem); // Closed value loaded from upvalues array
            }
            else
            {
                // Upvalue is a local originating from the current context.
                il.LoadLocal((ushort)src.Index);
            }
            // Store the upvalue to the new closure's upvalues array.
            il.Emit(OpCodes.Stelem);
        }
        
        il.Emit(OpCodes.Ldarg_0); // Load closure
        il.Emit(OpCodes.Ldfld, ReflectionRefs.Closure_Prototypes);
        il.LoadConstant(index);
        il.Emit(OpCodes.Ldelem); // Load the CompiledPrototype that refers to this function definition
        il.Emit(OpCodes.Ldfld, ReflectionRefs.CompiledPrototype_Prototypes); // Load its Prototypes
        
        il.Emit(OpCodes.Newobj, ReflectionRefs.Closure_Ctor); // Create the closure object
        
        il.Emit(OpCodes.Callvirt, ReflectionRefs.MethodInfo_CreateDelegate); // Create the delegate
        il.Emit(OpCodes.Castclass, typeof(LuaFunction)); // Cast delegate to correct type
        il.Emit(OpCodes.Call, ReflectionRefs.LuaValue_NewFunction); // Convert to LuaValue
    }

    internal override void Initialize(Stack<Expression> stack) { }

    public override string ToString()
        => base.ToString() + $" <{debugFunctionName ?? "anonymous"}" +
           $"({string.Join(", ", isVararg ? parameters.Append("...") : parameters)})>";
}
