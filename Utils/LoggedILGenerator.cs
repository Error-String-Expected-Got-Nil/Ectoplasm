using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;

namespace Ectoplasm.Utils;

/// <summary>
/// Wrapper around <see cref="ILGenerator"/> that logs any emitted instructions and their operands, and local variable
/// declarations. A human-friendly string can then be retrieved via the <see cref="GetLog"/> method. Useful as a way to
/// check that dynamic code generation is producing good IL without needing to save and disassemble the raw bytecode.
/// </summary>
/// <param name="orig">Original <see cref="ILGenerator"/> to wrap.</param>
// ReSharper disable once InconsistentNaming
public class LoggedILGenerator(ILGenerator orig) : ILGenerator
{
    private const int Padding = 3;
    
    private static readonly int MaxOpcodeStringLength
        = typeof(OpCodes).GetFields().Max(fi => ((OpCode)fi.GetValue(null)!).ToString()!.Length);

    private static readonly Dictionary<OpCode, string> PaddedOpcodeStrings = [];

    private readonly StringBuilder _log = new();
    private readonly List<string> _namespaces = [];
    private readonly List<LocalBuilder> _locals = [];

    private bool _indented;

    public string GetLog()
    {
        var str = new StringBuilder();

        if (_namespaces is { Count: > 0 })
        {
            foreach (var ns in _namespaces)
            {
                str.Append("using ");
                str.AppendLine(ns);
            }

            str.AppendLine();
        }

        if (_locals is { Count: > 0 })
        {
            foreach (var local in _locals)
            {
                str.Append('[');
                str.Append(local.LocalIndex);
                str.Append(']');
                str.Append(' ');
                str.Append(local.LocalType);
                if (local.IsPinned) str.Append(" pinned");
            }

            str.AppendLine();
        }

        str.Append(_log);

        return str.ToString();
    }
    
    private void Log(OpCode opcode, object? extra = null)
    {
        if (extra is null)
        {
            if (_indented) _log.Append("    ");
            _log.AppendLine(opcode.ToString());
            return;
        }

        if (!PaddedOpcodeStrings.TryGetValue(opcode, out var padded))
        {
            padded = opcode.ToString()!.PadRight(MaxOpcodeStringLength + Padding);
            PaddedOpcodeStrings.Add(opcode, padded);
        }

        if (_indented) _log.Append("    ");
        _log.Append(padded);
        _log.AppendLine(extra.ToString());
    }
    
    public override int ILOffset => orig.ILOffset;
    
    public override void Emit(OpCode opcode)
    {
        Log(opcode);
        orig.Emit(opcode);
    }

    public override void Emit(OpCode opcode, byte arg)
    {
        Log(opcode, arg);
        orig.Emit(opcode, arg);
    }

    public override void Emit(OpCode opcode, double arg)
    {
        Log(opcode, arg);
        orig.Emit(opcode, arg);
    }

    public override void Emit(OpCode opcode, short arg)
    {
        Log(opcode, arg);
        orig.Emit(opcode, arg);
    }

    public override void Emit(OpCode opcode, int arg)
    {
        Log(opcode, arg);
        orig.Emit(opcode, arg);
    }

    public override void Emit(OpCode opcode, long arg)
    {
        Log(opcode, arg);
        orig.Emit(opcode, arg);
    }

    public override void Emit(OpCode opcode, ConstructorInfo con)
    {
        Log(opcode, con);
        orig.Emit(opcode, con);
    }

    public override void Emit(OpCode opcode, Label label)
    {
        Log(opcode, $"[{label.Id}]");
        orig.Emit(opcode, label);
    }

    public override void Emit(OpCode opcode, Label[] labels)
    {
        var str = new StringBuilder();
        foreach (var label in labels)
        {
            str.Append('[');
            str.Append(label.Id);
            str.Append(']');
        }

        Log(opcode, str);
        orig.Emit(opcode, labels);
    }

    public override void Emit(OpCode opcode, LocalBuilder local)
    {
        Log(opcode, local);
        orig.Emit(opcode, local);
    }

    public override void Emit(OpCode opcode, SignatureHelper signature)
    {
        Log(opcode, signature);
        orig.Emit(opcode, signature);
    }

    public override void Emit(OpCode opcode, FieldInfo field)
    {
        Log(opcode, field);
        orig.Emit(opcode, field);
    }

    public override void Emit(OpCode opcode, MethodInfo meth)
    {
        Log(opcode, meth);
        orig.Emit(opcode, meth);
    }

    public override void Emit(OpCode opcode, float arg)
    {
        Log(opcode, arg);
        orig.Emit(opcode, arg);
    }

    public override void Emit(OpCode opcode, string str)
    {
        Log(opcode, str.GetEscapedString());
        orig.Emit(opcode, str);
    }

    public override void Emit(OpCode opcode, Type cls)
    {
        Log(opcode, cls);
        orig.Emit(opcode, cls);
    }
    
    public override void MarkLabel(Label loc)
    {
        throw new NotImplementedException();
    }

    public override void EmitCall(OpCode opcode, MethodInfo methodInfo, Type[]? optionalParameterTypes)
    {
        if (optionalParameterTypes is { Length: > 0 })
        {
            var str = new StringBuilder();
            str.Append(methodInfo);
            foreach (var param in optionalParameterTypes)
            {
                str.Append('<');
                str.Append(param);
                str.Append('>');
            }
            
            Log(opcode, str);
        }
        else
        {
            Log(opcode, methodInfo);
        }

        orig.EmitCall(opcode, methodInfo, optionalParameterTypes);
    }

    public override void EmitCalli(OpCode opcode, CallingConventions callingConvention, Type? returnType, 
        Type[]? parameterTypes, Type[]? optionalParameterTypes)
    {
        var str = new StringBuilder();

        str.Append('[');
        str.Append(callingConvention.ToString());
        str.Append(']');

        if (returnType is not null)
        {
            str.Append('{');
            str.Append(returnType);
            str.Append('}');
        }

        if (parameterTypes is { Length: > 0 })
        {
            str.Append('(');
            foreach (var type in parameterTypes)
            {
                str.Append('<');
                str.Append(type);
                str.Append('>');
            }
            str.Append(')');
        }

        if (optionalParameterTypes is { Length: > 0 })
        {
            foreach (var type in optionalParameterTypes)
            {
                str.Append('<');
                str.Append(type);
                str.Append('>');
            }
        }
        
        Log(opcode, str);
        orig.EmitCalli(opcode, callingConvention, returnType, parameterTypes, optionalParameterTypes);
    }

    public override void EmitCalli(OpCode opcode, CallingConvention unmanagedCallConv, Type? returnType, 
        Type[]? parameterTypes)
    {
        var str = new StringBuilder();

        str.Append('[');
        str.Append(unmanagedCallConv.ToString());
        str.Append(']');

        if (returnType is not null)
        {
            str.Append('{');
            str.Append(returnType);
            str.Append('}');
        }

        if (parameterTypes is { Length: > 0 })
        {
            str.Append('(');
            foreach (var type in parameterTypes)
            {
                str.Append('<');
                str.Append(type);
                str.Append('>');
            }
            str.Append(')');
        }
        
        Log(opcode, str);
        orig.EmitCalli(opcode, unmanagedCallConv, returnType, parameterTypes);
    }
    
    public override Label BeginExceptionBlock()
    {
        _log.AppendLine(".try");
        _log.AppendLine("{");
        _indented = true;
        
        return orig.BeginExceptionBlock();
    }
    
    public override void BeginCatchBlock(Type? exceptionType)
    {
        _log.AppendLine("}");
        _log.Append(".catch   ");
        _log.AppendLine(exceptionType?.ToString() ?? "");
        _log.AppendLine("{");
        
        orig.BeginCatchBlock(exceptionType);
    }

    public override void BeginExceptFilterBlock()
    {
        _log.AppendLine("{{{ BEGIN EXCEPTION FILTER BLOCK }}}");
        orig.BeginExceptFilterBlock();
    }

    public override void BeginFaultBlock()
    {
        _log.AppendLine("{{{ BEGIN FAULT BLOCK }}}");
        orig.BeginFaultBlock();
    }

    public override void BeginFinallyBlock()
    {
        _log.AppendLine("}");
        _log.AppendLine(".finally");
        _log.AppendLine("{");
        
        orig.BeginFinallyBlock();
    }
    
    public override void EndExceptionBlock()
    {
        _log.AppendLine("}");
        _indented = false;
        
        orig.EndExceptionBlock();
    }

    public override void BeginScope()
    {
        _log.AppendLine("{{{ BEGIN SCOPE }}}");
        orig.BeginScope();
    }
    
    public override void EndScope()
    {
        _log.AppendLine("{{{ END SCOPE }}}");
        orig.EndScope();
    }

    public override void UsingNamespace(string usingNamespace)
    {
        _namespaces.Add(usingNamespace);
        orig.UsingNamespace(usingNamespace);
    }
    
    public override LocalBuilder DeclareLocal(Type localType, bool pinned)
    {
        var ret = orig.DeclareLocal(localType, pinned);
        _locals.Add(ret);
        return ret;
    }

    public override Label DefineLabel() => orig.DefineLabel();
}