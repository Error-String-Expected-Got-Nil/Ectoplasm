namespace Ectoplasm.Lexing;

public class LuaLexingException(string message, ushort line, ushort col) 
    : Exception($"Error lexing input on line {line}, column {col}: {message}");