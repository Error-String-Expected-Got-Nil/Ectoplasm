namespace Ectoplasm.Lexing;

public class LexingException(string message, ushort line, ushort col) 
    : Exception($"Error lexing input on line {line + 1}, column {col + 1}: {message}");