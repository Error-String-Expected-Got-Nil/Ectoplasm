using Ectoplasm.Lexing;

namespace Ectoplasm.Parsing;

public class LuaParsingException : Exception
{
    public LuaParsingException(string message) : base(message) { }
    
    /// <summary>
    /// Throw a parsing exception, noting what line and column of the source it occured on.
    /// </summary>
    public LuaParsingException(string message, int line, int col) 
        : base($"Error parsing input on line {line}, column {col}: {message}") { }

    /// <summary>
    /// Throws an exception for an unexpected token with some optional additional context in parentheses afterward.
    /// </summary>
    public LuaParsingException(LuaToken offendingToken, string? context = null)
        : this($"Unexpected token '{offendingToken.OriginalOrPlaceholder}'" + (context is null ? "" : $" ({context})"), 
            offendingToken.StartLine, offendingToken.StartCol) { }
}