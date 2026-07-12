using Ectoplasm.Parsing.Expressions;
using Ectoplasm.Parsing.Statements;
using Ectoplasm.Utils;

namespace Ectoplasm.Parsing;

/// <summary>
/// Static class holding code for analyzing variable scopes in parsed chunks.
/// </summary>
public static class ScopeAnalyzer
{
    /// <summary>
    /// Analyze a list of statements as a main chunk, returning the function prototype for that chunk.
    /// </summary>
    public static Prototype AnalyzeChunk(List<Statement> chunk, string? sourceName)
    {
        var main = new Prototype(null, [], true, chunk, 0, 0, "<main chunk>", sourceName);
        var scopeStack = new TransparentStack<Scope>([new Scope(main)]);

        RecursiveAnalyze(scopeStack);
        
        return main;
    }

    /// <summary>
    /// Analyzes the top scope on the scope stack.
    /// </summary>
    private static void RecursiveAnalyze(TransparentStack<Scope> scopeStack)
    {
        var cur = scopeStack.Peek();
        var proto = cur.EnclosingPrototype;
        
        foreach (var stat in cur.Contents)
        {
            foreach (var expr in stat.GetExpressions() ?? [])
            foreach (var (node, _) in expr.DepthFirstEnumerate())
            {
                switch (node)
                {
                    case Expr_Variable variable:
                        LocateVariable(scopeStack, variable);
                        break;
                    case Expr_Varargs:
                        if (!proto.IsVararg)
                            throw new LuaParsingException($"Vararg expression used even though the containing " +
                                $"function (name: {proto.Name}, defined on line {proto.Line}, column {proto.Col}) " +
                                $"is not variadic", node.StartLine, node.StartCol, proto.SourceName);
                        break;
                    case Expr_FunctionDef def:
                        var newProto = new Prototype(proto, def, proto.SourceName);
                        var newScope = new Scope(newProto, newProto.Contents);
                        scopeStack.Push(newScope);
                        RecursiveAnalyze(scopeStack);
                        break;
                }
            }

            foreach (var local in stat.DeclareLocals(proto) ?? [])
                cur.DeclaredNames[local.Name] = local;

            foreach (var (block, locals) in stat.GetBlocks() ?? [])
            {
                var next = new Scope(proto, block);
                foreach (var local in locals ?? []) next.DeclaredNames[local.Name] = local;
                scopeStack.Push(next);
                RecursiveAnalyze(scopeStack);
            }

            // TODO: Handle any statement type-specific behavior like assignment checking for <const>
            //  Goto needs to look for its target label and check if there is a declaration mismatch
            //  Labels need to make sure they aren't shadowing a higher-scope label
            //  Both need to have their visible locals logged

            scopeStack.Pop();
        }
    }

    /// <summary>
    /// Searches the scope stack to find what variable a variable expression is referencing, and sets the variable
    /// expression accordingly. If the located variable is an external local, automatically marks it as an upvalue and
    /// propagates it through the prototype scope chain as an external name. 
    /// </summary>
    private static void LocateVariable(TransparentStack<Scope> scopeStack, Expr_Variable variable)
    {
        var startProto = scopeStack.Peek().EnclosingPrototype;
        
        // Easy case: The variable referenced is a local declared in the same scope.
        if (scopeStack.Peek().DeclaredNames.TryGetValue(variable.Name, out var localEasy))
        {
            variable.Source = localEasy;
            return;
        }
        
        // More complicated case: The variable referenced is either global or from a higher scope. We will need to
        // prepare for the case that it is an external local by tracking what prototype scopes we pass through
        // while searching.
        var passedPrototypeScopes = new Stack<Scope>();
        LocalVariable? foundVariable = null;

        foreach (var scope in scopeStack.EnumerateTopDown())
        {
            if (scope.DeclaredNames.TryGetValue(variable.Name, out var local))
            {
                // The referenced variable was indeed a local. If it's from the same prototype we started in, then this
                // is still easy, otherwise it was external, and we need additional handling.
                if (scope.EnclosingPrototype == startProto)
                {
                    variable.Source = local;
                    return;
                }
                
                // It's external, note it and break to handle.
                foundVariable = local;
                break;
            }
            
            if (scope.IsPrototypeRoot) passedPrototypeScopes.Push(scope);
        }

        // Variable was not located in any scope all the way up to the top, it's a global variable.
        if (foundVariable is null)
        {
            variable.IsGlobal = true;
            return;
        }
        
        // Found variable is external. Mark is as an upvalue and then add a chain of external variable references
        // through every prototype passed on the way to it.
        foundVariable.IsUpvalue = true;
        var prevInChain = foundVariable;
        foreach (var scope in passedPrototypeScopes)
            prevInChain = scope.EnclosingPrototype.AddNewExternal(prevInChain);

        variable.Source = prevInChain;
    }
}