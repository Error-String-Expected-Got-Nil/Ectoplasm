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
        var main = new Prototype(null, [], true, chunk, "<main chunk>", sourceName);
        var scopeStack = new TransparentStack<Scope>([new Scope(main)]);

        RecursiveAnalyze(scopeStack);
        
        return main;
    }

    /// <summary>
    /// Analyzes the top scope on the scope stack.
    /// </summary>
    private static void RecursiveAnalyze(TransparentStack<Scope> scopeStack)
    {
        
    }
}