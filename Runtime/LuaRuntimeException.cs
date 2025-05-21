namespace Ectoplasm.Runtime;

/// <summary>
/// Represents exceptions caused by Lua-specific errors.
/// </summary>
/// <param name="message">Message describing reason for exception.</param>
public class LuaRuntimeException(string message) : Exception(message);