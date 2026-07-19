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
                        proto.Children.Add(newProto);
                        def.Prototype = newProto;
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
                var next = new Scope(proto, block, stat.IsBreakable);
                foreach (var local in locals ?? []) next.DeclaredNames[local.Name] = local;
                scopeStack.Push(next);
                RecursiveAnalyze(scopeStack);
            }

            switch (stat)
            {
                case Stat_Assign assign:
                    foreach (var expr in assign.Variables)
                        if (!expr.IsAssignable)
                            throw new LuaParsingException("Target expression of assignment statement was found to be " +
                                "non-assignable during analysis; most likely, it was resolved as a local variable with " +
                                "the <const> or <close> attribute.", expr.StartLine, expr.StartCol, proto.SourceName);
                    break;
                case Stat_Goto statGoto:
                    ValidateGoto(statGoto, scopeStack);
                    break;
                case Stat_Label label:
                    ValidateLabel(label, scopeStack);
                    break;
                case Stat_Break statBreak:
                    foreach (var scope in scopeStack.EnumerateTopDown())
                    {
                        if (scope.IsBreakable) break;
                        if (scope.IsPrototypeRoot)
                            throw new LuaParsingException("Attempt to break outside breakable scope", 
                                statBreak.StartLine, statBreak.StartCol, proto.SourceName);
                    }
                    break;
            }

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

    // Finds the label a goto refers to and, if possible at this point, verifies if it is a valid jump target.
    private static void ValidateGoto(Stat_Goto statGoto, TransparentStack<Scope> scopeStack)
    {
        var proto = scopeStack.Peek().EnclosingPrototype;

        // Local variables that matter for jump validation are any that are currently visibly declared from the current 
        // scope up to root prototype scope. 
        statGoto.VisibleLocals = [];
        foreach (var scope in scopeStack.EnumerateTopDown())
        {
            foreach (var (_, local) in scope.DeclaredNames) statGoto.VisibleLocals.Add(local);
            if (scope.IsPrototypeRoot) break;
        }
        proto.Gotos.Add(statGoto);

        foreach (var scope in scopeStack.EnumerateTopDown())
        {
            if (scope.DeclaredLabels.TryGetValue(statGoto.TargetLabel, out var label))
            {
                statGoto.ResolvedTarget = label;
                if (label.IsTerminal || label.VisibleLocals is not { } visibleAtTarget) return;
                var skipped = new HashSet<LocalVariable>(visibleAtTarget);
                skipped.ExceptWith(statGoto.VisibleLocals);
                if (skipped.Count > 0)
                    throw new LuaParsingException($"Jump to label on line {label.StartLine}, column {label.StartCol} " +
                        $"is not valid, as it would bypass one or more local variable declarations: " +
                        $"{string.Join(", ", skipped.Select(local => local.Name))}",
                        statGoto.StartLine, statGoto.StartCol, proto.SourceName);
            }

            if (scope.IsPrototypeRoot)
                throw new LuaParsingException($"No visible label had target name \"{statGoto.TargetLabel}\" for goto " +
                    $"statement", statGoto.StartLine, statGoto.StartCol, proto.SourceName);
        }
    }

    // Sets the locals visible at a label and checks if any previously found gotos targeted it, then checks that their
    // jumps to that label are valid. Also ensures that a label does not shadow any other visible labels.
    private static void ValidateLabel(Stat_Label label, TransparentStack<Scope> scopeStack)
    {
        var proto = scopeStack.Peek().EnclosingPrototype;

        label.VisibleLocals = [];
        foreach (var scope in scopeStack.EnumerateTopDown())
        {
            foreach (var (_, local) in scope.DeclaredNames) label.VisibleLocals.Add(local);
            if (scope.IsPrototypeRoot) break;
        }

        foreach (var scope in scopeStack.EnumerateTopDown())
        {
            // Labels are not allowed to shadow each other so we check that here, unless this is the scope this label
            // comes from, because this was already validated for that scope when that scope was created.
            if (scope != scopeStack.Peek() && scope.DeclaredLabels.TryGetValue(label.LabelName, out var other))
                throw new LuaParsingException($"Label would shadow other label with same name declared on line " +
                    $"{other.StartLine}, column {other.StartCol}", label.StartLine, label.StartCol, proto.SourceName);

            // Check any gotos targeting this label to ensure their jumps are valid, if applicable.
            if (!label.IsTerminal)
            {
                foreach (var statGoto in proto.Gotos.Where(statGoto => statGoto.ResolvedTarget == label))
                {
                    var skipped = new HashSet<LocalVariable>(label.VisibleLocals);
                    // A goto with a non-null ResolvedTarget will always have its visible locals set.
                    skipped.ExceptWith(statGoto.VisibleLocals!);
                    if (skipped.Count > 0)
                        throw new LuaParsingException($"Jump to label on line {label.StartLine}, column " +
                            $"{label.StartCol} is not valid, as it would bypass one or more local variable " +
                            $"declarations: {string.Join(", ", skipped.Select(local => local.Name))}",
                            statGoto.StartLine, statGoto.StartCol, proto.SourceName);
                }
            }
            
            if (scope.IsPrototypeRoot) break;
        }
    }
}