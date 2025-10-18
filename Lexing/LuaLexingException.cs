namespace Ectoplasm.Lexing;

public class LuaLexingException(string message, ushort line, ushort col, string? sourceName = null) 
    : Exception("Error lexing input " 
                + (sourceName is null ? "" : $"from source '{sourceName}' ")
                + $"on line {line}, column {col}: {message}");