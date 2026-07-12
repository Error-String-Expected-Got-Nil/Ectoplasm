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
    public static Prototype AnalyzeChunk(List<Statement> chunk)
    {
        var main = new Prototype(null, [], true, chunk, "<main chunk>");
        var scopeStack = new TransparentStack<Scope>([new Scope(main)]);

        RecursiveAnalyzeBlock(scopeStack, chunk);
        
        return main;
    }

    private static void RecursiveAnalyzeBlock(TransparentStack<Scope> scopeStack, List<Statement> block)
    {
        // TODO
    }
}