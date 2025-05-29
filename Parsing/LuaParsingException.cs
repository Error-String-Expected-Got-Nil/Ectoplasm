namespace Ectoplasm.Parsing;

public class LuaParsingException : Exception
{
    public LuaParsingException(string message) : base(message) { }
    
    public LuaParsingException(string message, int line, int col) 
        : base($"Error parsing input on line {line}, column {col}: {message}") { }
}